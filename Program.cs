using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SisieSecretKey2026!@#$%";
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

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Servicios
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// ============================================
// CONFIGURACIÓN DEL PIPELINE
// ============================================

// Migration automática + Seed de datos
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
    
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
    
    if (!db.Productos.Any())
    {
        // Obtener categorías creadas
        var cats = db.Categorias.ToList();
        
        if (cats.Count >= 5)
        {
            var productos = new List<proyecto_SISIE.Models.Entities.Producto>
            {
                new() { Nombre = "Martillo de Acero", Descripcion = "Martillo de acero 300g", Precio = 4500, Stock = 15, IdCategoria = cats[0].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { Nombre = "Destornillador Phillips", Descripcion = "Destornillador estrella profesional", Precio = 1200, Stock = 25, IdCategoria = cats[0].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { Nombre = "Llave Combinada 10mm", Descripcion = "Llave de acero cromo vanadio", Precio = 2800, Stock = 8, IdCategoria = cats[0].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { Nombre = "Cable RG6 100m", Descripcion = "Cable coaxial para TV y antena", Precio = 8500, Stock = 12, IdCategoria = cats[1].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { Nombre = "Interruptor Simple", Descripcion = "Interruptor de embutir blanco", Precio = 850, Stock = 50, IdCategoria = cats[1].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { Nombre = "Tomada 2 Pines", Descripcion = "Tomada corriente 10A", Precio = 650, Stock = 40, IdCategoria = cats[1].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { Nombre = "Tubo PVC 3m", Descripcion = "Tubo de presión 20mm", Precio = 2200, Stock = 30, IdCategoria = cats[2].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { Nombre = "Grifo de Cocina", Descripcion = "Grifo metálico cromado", Precio = 12500, Stock = 5, IdCategoria = cats[2].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { Nombre = "Pintura Látex 20L", Descripcion = "Pintura blanca interior", Precio = 18500, Stock = 10, IdCategoria = cats[3].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { Nombre = "Rodillo de Lana", Descripcion = "Rodillo para pintura", Precio = 1800, Stock = 20, IdCategoria = cats[3].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { Nombre = "Tornillos Madera x50", Descripcion = "Tornillos de 4cm zincados", Precio = 850, Stock = 100, IdCategoria = cats[4].Id, FechaCreacion = DateTime.Now, Activo = true },
                new() { Nombre = "Clavos de Acero x100", Descripcion = "Clavos comunes 2 pulgadas", Precio = 450, Stock = 80, IdCategoria = cats[4].Id, FechaCreacion = DateTime.Now, Activo = true }
            };
            db.Productos.AddRange(productos);
        }
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

// Auth
app.UseAuthentication();
app.UseAuthorization();

// Controllers API
app.MapControllers();

// Página por defecto del Frontend
app.MapFallbackToFile("index.html");

app.Run();