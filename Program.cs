using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using AspNetCoreRateLimit;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;
using proyecto_SISIE.Services.Implementations;
using proyecto_SISIE.Services.Repositorios;
using proyecto_SISIE.Services.Strategy;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// CONFIGURACIÓN DE SERVICIOS
// ============================================

// Base de datos - SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
        "JWT Key no configurada. Crear variable de entorno Jwt__Key o agregarla en appsettings.json");
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

// Repositorios
builder.Services.AddScoped<IProductoRepositorio, ProductoRepositorio>();
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IClienteRepositorio, ClienteRepositorio>();
builder.Services.AddScoped<IVentaRepositorio, VentaRepositorio>();

// Servicios
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IClienteService, ClienteService>();

// Estrategias de pago (patrón Strategy)
builder.Services.AddScoped<IMetodoPagoStrategy, EfectivoStrategy>();
builder.Services.AddScoped<IMetodoPagoStrategy, TarjetaStrategy>();
builder.Services.AddScoped<IMetodoPagoStrategy, TransferenciaStrategy>();
builder.Services.AddScoped<ProcesadorPago>();

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
        
        // ================================================
        // CREACIÓN DE BASE DE DATOS Y TABLAS
        // ================================================
        // EnsureCreated falla si el usuario no tiene permiso CREATE DATABASE
        // aunque la DB ya exista. Probamos con conexión directa a SISIE:
        // si podemos conectar, creamos tablas via el modelo EF.
        // ================================================
        var sisieConnStr = db.Database.GetConnectionString();
        var schemaReady = false;

        using (var testConn = new SqlConnection(sisieConnStr))
        {
            try
            {
                testConn.Open();

                // Conexión exitosa → verificar si tiene tablas
                using var checkCmd = testConn.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM sys.tables";
                var tableCount = (int)checkCmd.ExecuteScalar();

                if (tableCount == 0)
                {
                    // No hay tablas → usar EnsureCreated pero con flag
                    // para que SKIP el CREATE DATABASE
                    var creator = db.Database.GetService<IRelationalDatabaseCreator>();
                    creator.CreateTables();
                }

                // Diagnóstico: listar tablas existentes
                using var diagCmd = testConn.CreateCommand();
                diagCmd.CommandText = "SELECT TABLE_SCHEMA + '.' + TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
                using var reader = diagCmd.ExecuteReader();
                var tablas = new List<string>();
                while (reader.Read()) tablas.Add(reader.GetString(0));
                reader.Close();
                Console.WriteLine($"[DBG] Tablas en BD ({tablas.Count}): {string.Join(", ", tablas)}");

                schemaReady = true;
            }
            catch (SqlException ex) when (ex.Number == 4060 || ex.Number == 18456)
            {
                // 4060 = cannot open database (no CONNECT)
                // 18456 = login failed
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[WARN] No se puede conectar a SISIE: {ex.Message}");
                Console.ResetColor();
            }
            catch (InvalidOperationException ex)
            {
                // Si GetService<IRelationalDatabaseCreator> falla,
                // usamos EnsureCreated normal como respaldo (raro)
                Console.WriteLine($"[WARN] Usando EnsureCreated como respaldo: {ex.Message}");
                try { db.Database.EnsureCreated(); schemaReady = true; }
                catch { /* ignorar */ }
            }
        }

        if (!schemaReady)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("============================================================================");
            Console.WriteLine("  No se pudo inicializar la base de datos automáticamente."                   );
            Console.WriteLine("  Abrí SSMS, conectate a .\\SQLEXPRESS y ejecutá el script:"                  );
            Console.WriteLine("    C:\\Users\\flia\\Desktop\\script-migracion-sqlserver.sql"                 );
            Console.WriteLine("  Luego reiniciá la aplicación."                                             );
            Console.WriteLine("============================================================================");
            Console.ResetColor();
            return;
        }

        // Ejecutar SPs, triggers y transacciones (solo si no existen aun)
        EjecutarSqlProgrammable(db);
    
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

    // Seed de métodos de pago
    if (!db.MetodosPago.Any())
    {
        db.MetodosPago.AddRange(
            new proyecto_SISIE.Models.Entities.MetodoPago { Nombre = "Efectivo", RecargoPorcentaje = 0, Activo = true },
            new proyecto_SISIE.Models.Entities.MetodoPago { Nombre = "Tarjeta", RecargoPorcentaje = 3, Activo = true },
            new proyecto_SISIE.Models.Entities.MetodoPago { Nombre = "Transferencia", RecargoPorcentaje = 1.5m, Activo = true }
        );
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

// Abrir navegador automáticamente después de iniciar
_ = Task.Run(async () =>
{
    await Task.Delay(2000);
    try
    {
        Process.Start(new ProcessStartInfo("http://localhost:5000") { UseShellExecute = true });
    }
    catch { /* Si falla, el usuario abre el navegador manual */ }
});

app.Run();

// ============================================
// EJECUCIÓN DE SPs, TRIGGERS Y TRANSACCIONES
// ============================================
static void EjecutarSqlProgrammable(ApplicationDbContext db)
{
    // Usamos ADO.NET directo porque ExecuteSqlRaw puede fallar con
    // CREATE PROCEDURE debido al manejo de batches.
    var connStr = db.Database.GetConnectionString();
    using var sqlConn = new SqlConnection(connStr);
    sqlConn.Open();
    // NOTA: Todos los nombres de tablas y columnas deben coincidir
    // con lo que genera EF Core (PascalCase, pluralizados).
    // Tables: Ventas, DetallesVenta, Productos, Categorias, Usuarios, etc.
    //
    // Cada SQL se ejecuta como batch independiente. Los SPs se crean
    // con ADO.NET directo (no ExecuteSqlRaw) porque CREATE PROCEDURE
    // necesita ser la única instrucción del batch.
    var tablesAndTriggers = new[]
    {
        // ===== TABLA DE AUDITORÍA =====
        @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditoriaVenta')
CREATE TABLE dbo.AuditoriaVenta (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdVenta INT NOT NULL FOREIGN KEY REFERENCES dbo.Ventas(Id),
    EstadoAnterior VARCHAR(20),
    EstadoNuevo VARCHAR(20) NOT NULL,
    Usuario VARCHAR(100),
    FechaCambio DATETIME2 DEFAULT GETDATE()
)",
        // ===== TRIGGER: Auditoría de cambios de estado =====
        @"
CREATE TRIGGER trg_Audit_VentaEstado
ON dbo.Ventas AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF UPDATE(Estado)
    BEGIN
        INSERT INTO AuditoriaVenta (IdVenta, EstadoAnterior, EstadoNuevo, Usuario)
        SELECT i.Id, d.Estado, i.Estado, SYSTEM_USER
        FROM inserted i INNER JOIN deleted d ON i.Id = d.Id
        WHERE i.Estado <> d.Estado;
    END
END",
        // ===== TRIGGER: Prevenir modificar entregadas/canceladas =====
        // NOTA: INSTEAD OF no es compatible con FKs CASCADE en SQL Server.
        @"
CREATE TRIGGER trg_PreventCancelarEntregada
ON dbo.Ventas AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF UPDATE(Estado)
    BEGIN
        IF EXISTS (SELECT 1 FROM deleted WHERE Estado = 'Entregada')
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 50002, 'No se puede modificar una venta que ya fue entregada.', 1;
        END
        IF EXISTS (SELECT 1 FROM deleted d WHERE d.Estado = 'Cancelada'
            AND EXISTS (SELECT 1 FROM inserted i WHERE i.Id = d.Id AND i.Estado <> 'Cancelada'))
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 50003, 'No se puede reactivar una venta cancelada.', 1;
        END
    END
END"
    };

    // Helper: drop object si existe (compatible SQL Server 2008+)
    void DropIfExists(string type, string schema, string name)
    {
        using var dropCmd = sqlConn.CreateCommand();
        dropCmd.CommandText = $"IF OBJECT_ID('{schema}.{name}', '{type}') IS NOT NULL DROP {type} {schema}.{name}";
        dropCmd.ExecuteNonQuery();
    }

    // Dropear triggers existentes antes de recrearlos
    DropIfExists("TRIGGER", "dbo", "trg_Audit_VentaEstado");
    DropIfExists("TRIGGER", "dbo", "trg_PreventCancelarEntregada");

    // Crear objetos (tablas de auditoría, triggers)
    foreach (var sql in tablesAndTriggers)
    {
        try
        {
            using var cmd = sqlConn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SQL Programmable] Info: {ex.Message}");
        }
    }

    // Los SPs: primero DROP si existen, luego CREATE
    // (CREATE OR ALTER no está disponible en algunas versiones de SQL Server)
    var spDefs = new (string name, string sql)[]
    {
        // ===== SP: Registrar Venta (transaccional) =====
        ("sp_RegistrarVenta", @"
CREATE PROCEDURE sp_RegistrarVenta
    @NumeroVenta INT, @Descuento INT, @IdMetodoPago INT, @TipoEntrega VARCHAR(30),
    @Estado VARCHAR(20) = 'Pendiente', @Notas VARCHAR(200) = NULL, @IdDireccion INT = NULL,
    @IdUsuario INT, @Total DECIMAL(18,2) = 0
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @IdVenta INT, @ErrMsg NVARCHAR(4000);
    BEGIN TRY
        BEGIN TRANSACTION;
        INSERT INTO dbo.Ventas (NumeroVenta, Descuento, IdMetodoPago, TipoEntrega, Estado,
            Notas, FechaCreacion, IdDireccion, IdUsuario, Total)
        VALUES (@NumeroVenta, @Descuento, @IdMetodoPago, @TipoEntrega, @Estado,
            @Notas, GETDATE(), @IdDireccion, @IdUsuario, @Total);
        SET @IdVenta = SCOPE_IDENTITY();
        SELECT @IdVenta AS IdVenta;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        SET @ErrMsg = ERROR_MESSAGE();
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW 50010, @ErrMsg, 1;
    END CATCH;
END"),
        // ===== SP: Registrar Detalle Venta =====
        ("sp_RegistrarDetalleVenta", @"
CREATE PROCEDURE sp_RegistrarDetalleVenta
    @IdVenta INT, @IdProducto INT, @Cantidad INT, @PrecioUnitario DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrMsg NVARCHAR(4000), @StockActual INT;
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @StockActual = Stock FROM dbo.Productos WHERE Id = @IdProducto;
        IF @StockActual IS NULL THROW 50020, 'Producto no encontrado.', 1;
        IF @StockActual < @Cantidad
        BEGIN
            SET @ErrMsg = 'Stock insuficiente. Disp: ' + CAST(@StockActual AS VARCHAR) + ', req: ' + CAST(@Cantidad AS VARCHAR);
            THROW 50021, @ErrMsg, 1;
        END
        INSERT INTO dbo.DetallesVenta (IdVenta, IdProducto, Cantidad, PrecioUnitario, SubTotal)
        VALUES (@IdVenta, @IdProducto, @Cantidad, @PrecioUnitario, @Cantidad * @PrecioUnitario);
        SELECT SCOPE_IDENTITY() AS Id;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        SET @ErrMsg = ERROR_MESSAGE();
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW 50022, @ErrMsg, 1;
    END CATCH;
END"),
        // ===== SP: Cancelar Venta con restauración de stock =====
        ("sp_CancelarVenta", @"
CREATE PROCEDURE sp_CancelarVenta
    @IdVenta INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EstadoActual VARCHAR(20), @ErrMsg NVARCHAR(4000);
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @EstadoActual = Estado FROM Ventas WHERE Id = @IdVenta;
        IF @EstadoActual IS NULL THROW 50030, 'Venta no encontrada.', 1;
        IF @EstadoActual = 'Cancelada' THROW 50031, 'La venta ya está cancelada.', 1;
        IF @EstadoActual = 'Entregada' THROW 50032, 'No se puede cancelar una venta entregada.', 1;
        UPDATE p SET Stock = p.Stock + dv.Cantidad
        FROM dbo.Productos p INNER JOIN dbo.DetallesVenta dv ON p.Id = dv.IdProducto
        WHERE dv.IdVenta = @IdVenta;
        UPDATE dbo.Ventas SET Estado = 'Cancelada' WHERE Id = @IdVenta;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        SET @ErrMsg = ERROR_MESSAGE();
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW 50033, @ErrMsg, 1;
    END CATCH;
END"),
        // ===== SP: Historial paginado (consulta) =====
        ("sp_ObtenerHistorialVentas", @"
CREATE PROCEDURE sp_ObtenerHistorialVentas
    @Pagina INT = 1, @TamanoPagina INT = 10, @IdUsuario INT = NULL,
    @Estado VARCHAR(20) = NULL, @FechaDesde DATETIME2 = NULL, @FechaHasta DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@Pagina - 1) * @TamanoPagina;
    SELECT COUNT(*) AS Total FROM dbo.Ventas v
    WHERE (@IdUsuario IS NULL OR v.IdUsuario = @IdUsuario)
      AND (@Estado IS NULL OR v.Estado = @Estado)
      AND (@FechaDesde IS NULL OR v.FechaCreacion >= @FechaDesde)
      AND (@FechaHasta IS NULL OR v.FechaCreacion <= @FechaHasta);
    SELECT v.Id, v.NumeroVenta, v.Estado, v.Total,
        mp.Nombre AS NombreMetodoPago, v.FechaCreacion,
        (SELECT COUNT(*) FROM dbo.DetallesVenta dv WHERE dv.IdVenta = v.Id) AS CantidadItems
    FROM dbo.Ventas v
    LEFT JOIN dbo.MetodosPago mp ON v.IdMetodoPago = mp.Id
    WHERE (@IdUsuario IS NULL OR v.IdUsuario = @IdUsuario)
      AND (@Estado IS NULL OR v.Estado = @Estado)
      AND (@FechaDesde IS NULL OR v.FechaCreacion >= @FechaDesde)
      AND (@FechaHasta IS NULL OR v.FechaCreacion <= @FechaHasta)
    ORDER BY v.FechaCreacion DESC
    OFFSET @Offset ROWS FETCH NEXT @TamanoPagina ROWS ONLY;
END"),
        // ===== SP: Estadísticas (consulta) =====
        ("sp_ObtenerEstadisticasVentas", @"
CREATE PROCEDURE sp_ObtenerEstadisticasVentas
    @FechaDesde DATETIME2 = NULL, @FechaHasta DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) AS TotalVentas, ISNULL(SUM(Total), 0) AS TotalFacturado,
        COUNT(CASE WHEN Estado = 'Cancelada' THEN 1 END) AS VentasCanceladas,
        COUNT(CASE WHEN Estado = 'Pendiente' THEN 1 END) AS VentasPendientes,
        COUNT(CASE WHEN Estado = 'Entregada' THEN 1 END) AS VentasEntregadas,
        @FechaDesde AS FechaDesde, @FechaHasta AS FechaHasta
    FROM dbo.Ventas
    WHERE (@FechaDesde IS NULL OR FechaCreacion >= @FechaDesde)
      AND (@FechaHasta IS NULL OR FechaCreacion <= @FechaHasta);
END")
    };

    // Primero dropear todos los SPs si existen, luego crearlos
    foreach (var (name, _) in spDefs)
        DropIfExists("PROCEDURE", "dbo", name);

    foreach (var (_, sql) in spDefs)
    {
        try
        {
            using var cmd = sqlConn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
            Console.WriteLine($"[DBG] SP creado correctamente");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SQL Programmable] Error al crear SP: {ex.Message}");
        }
    }

    // Verificar que los SPs se crearon correctamente
    try
    {
        using var verCmd = sqlConn.CreateCommand();
        verCmd.CommandText = @"
SELECT name, 
       SUBSTRING(OBJECT_DEFINITION(OBJECT_ID('dbo.' + name)), 1, 200) AS def_preview
FROM sys.procedures 
WHERE name LIKE 'sp_%'
ORDER BY name";
        using var verReader = verCmd.ExecuteReader();
        while (verReader.Read())
        {
            var spName = verReader.GetString(0);
            var def = verReader.IsDBNull(1) ? "(SIN DEFINICIÓN)" : verReader.GetString(1);
            Console.WriteLine($"[DBG] SP {spName}: {def.Replace("\r\n", " ").Replace("\n", " ")}");
        }
        verReader.Close();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DBG] Error al verificar SPs: {ex.Message}");
    }

    sqlConn.Close();
}
