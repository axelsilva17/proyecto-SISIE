using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AspNetCoreRateLimit;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services;
using proyecto_SISIE.Services.Interfaces;
using proyecto_SISIE.Services.Implementations;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// CONFIGURACIÓN DE SERVICIOS
// ============================================

// Base de datos - SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] 
    ?? throw new InvalidOperationException(
        "JWT Key no configurada en appsettings.json");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "proyecto-SISIE";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "proyecto-SISIE";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Controllers con Views (para static files)
builder.Services.AddControllersWithViews();

// CORS - permitir todo durante desarrollo, restringir en producción
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        // TODO: En producción, restringir a dominios específicos:
        // policy.WithOrigins("https://tudominio.com", "https://www.tudominio.com");
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Rate Limiting - limitar peticiones para seguridad
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",  // todas las rutas
            Period = "1m",    // por minuto
            Limit = 100      // máximo 100 requests por minuto
        },
        new RateLimitRule
        {
            Endpoint = "post:/api/auth/login",  // solo login
            Period = "1m",
            Limit = 20   // máximo 20 intentos de login por minuto
        },
        new RateLimitRule
        {
            Endpoint = "post:/api/auth/register",  // solo registro
            Period = "1m",
            Limit = 15    // máximo 15 registros por minuto
        }
    };
});
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Servicios
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IClienteService, ClienteService>();

// Validadores (desacoplados)
builder.Services.AddScoped<IValidadorProducto, ValidadorProducto>();
builder.Services.AddScoped<IValidadorCategoria, ValidadorCategoria>();
builder.Services.AddScoped<IValidadorVenta, ValidadorVenta>();
builder.Services.AddScoped<IValidadorCliente, ValidadorCliente>();
builder.Services.AddScoped<IValidadorAuth, ValidadorAuth>();

var app = builder.Build();

// ============================================
// CONFIGURACIÓN DEL PIPELINE
// ============================================

    // Migraciones automáticas + Seed de datos
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // Aplicar migraciones (Identity + modelo) en vez de EnsureCreated
        db.Database.Migrate();
    
    // Seed de datos si no existen
    if (!db.Categorias.Any())
    {
        var categorias = new List<proyecto_SISIE.Models.Entities.Categoria>
        {
            new() { NombreCategoria = "Herramientas Manuales" },
            new() { NombreCategoria = "Electricidad" },
            new() { NombreCategoria = "Fontanería" },
            new() { NombreCategoria = "Pintura" },
            new() { NombreCategoria = "Fijaciones" }
        };
        db.Categorias.AddRange(categorias);
        db.SaveChanges();
    }
    
    // Seed de productos
    if (!db.Productos.Any())
    {
        var cats = db.Categorias.ToList();
        if (cats.Count >= 5)
        {
            var productos = new List<proyecto_SISIE.Models.Entities.Producto>
            {
                new() { NombreProducto = "Martillo de Acero", Descripcion = "Martillo de acero 300g", PrecioUnitario = 4500, Stock = 15, IdCategoria = cats[0].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { NombreProducto = "Destornillador Phillips", Descripcion = "Destornillador estrella", PrecioUnitario = 1200, Stock = 25, IdCategoria = cats[0].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { NombreProducto = "Llave Combinada 10mm", Descripcion = "Llave de acero cromo", PrecioUnitario = 2800, Stock = 8, IdCategoria = cats[0].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { NombreProducto = "Cable RG6 100m", Descripcion = "Cable coaxial para TV", PrecioUnitario = 8500, Stock = 12, IdCategoria = cats[1].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { NombreProducto = "Interruptor Simple", Descripcion = "Interruptor embutir blanco", PrecioUnitario = 850, Stock = 50, IdCategoria = cats[1].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { NombreProducto = "Tomada 2 Pines", Descripcion = "Tomada corriente 10A", PrecioUnitario = 650, Stock = 40, IdCategoria = cats[1].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { NombreProducto = "Tubo PVC 3m", Descripcion = "Tubo de presión 20mm", PrecioUnitario = 2200, Stock = 30, IdCategoria = cats[2].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { NombreProducto = "Grifo de Cocina", Descripcion = "Grifo metálico cromado", PrecioUnitario = 12500, Stock = 5, IdCategoria = cats[2].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { NombreProducto = "Pintura Látex 20L", Descripcion = "Pintura blanca interior", PrecioUnitario = 18500, Stock = 10, IdCategoria = cats[3].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { NombreProducto = "Rodillo de Lana", Descripcion = "Rodillo para pintura", PrecioUnitario = 1800, Stock = 20, IdCategoria = cats[3].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { NombreProducto = "Tornillos Madera x50", Descripcion = "Tornillos de 4cm zinc", PrecioUnitario = 850, Stock = 100, IdCategoria = cats[4].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { NombreProducto = "Clavos de Acero x100", Descripcion = "Clavos comunes 2pulg", PrecioUnitario = 450, Stock = 80, IdCategoria = cats[4].Id, FechaCreacion = DateTime.Now, Activo = true }
            };
            db.Productos.AddRange(productos);
        }
    }

    // Seed de contactos y usuarios: necesarios para la FK de Venta.IdUsuario
    // (Venta apunta a la tabla Usuarios, NO a AspNetUsers de Identity)
    if (!db.Contactos.Any())
    {
        db.Contactos.Add(new proyecto_SISIE.Models.Entities.Contacto
        {
            Email = "admin@sisie.com",
            Telefono = 987000000
        });
        db.SaveChanges();
    }

    // Seed de usuarios de prueba (necesario para ventas)
    if (!db.Usuarios.Any())
    {
        db.Usuarios.Add(new proyecto_SISIE.Models.Entities.Usuario
        {
            NombreUsuario = "admin",
            PasswordHash = "admin123",
            FechaCreacion = DateTime.Now,
            Activo = true,
            IdContacto = 1
        });
        db.SaveChanges();
    }

    // Seed de clientes de prueba
    if (!db.Clientes.Any())
    {
        var clientes = new List<proyecto_SISIE.Models.Entities.Cliente>
        {
            new() { Dni = "12345678", Nombre = "Juan Perez", Telefono = "0981123456", Email = "juan@test.com", FechaCreacion = DateTime.Now, Activo = true },
            new() { Dni = "87654321", Nombre = "Maria Gonzalez", Telefono = "0985987654", Email = "maria@test.com", FechaCreacion = DateTime.Now, Activo = true },
            new() { Dni = "11223344", Nombre = "Pedro Rodriguez", Telefono = "0971122334", Email = "pedro@test.com", FechaCreacion = DateTime.Now, Activo = true }
        };
        db.Clientes.AddRange(clientes);
    }
    db.SaveChanges();
}

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Archivos estáticos (wwwroot) - DEBE IR ANTES de routing
app.UseDefaultFiles();
app.UseStaticFiles();

// Routing
app.UseRouting();

// CORS
app.UseCors("AllowAll");

// Rate Limiting
app.UseIpRateLimiting();

// Auth
app.UseAuthentication();
app.UseAuthorization();

// Controllers API
app.MapControllers();

// Página por defecto del Frontend
app.MapFallbackToFile("index.html");

app.Run();
