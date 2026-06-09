-- ============================================================================
-- SCRIPT COMPLETO: Creación de base de datos SISIE (esquema + datos + SPs)
-- ============================================================================
-- Ejecutar completo en SSMS contra .\SQLEXPRESS
-- Crea la BD, todas las tablas, seed data, SPs y triggers.
-- ============================================================================

-- =============================================
-- CREAR BASE DE DATOS (si no existe)
-- =============================================
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SISIE')
    CREATE DATABASE SISIE;
GO

USE SISIE;
GO

-- =============================================
-- TABLAS PRINCIPALES
-- =============================================

-- Contactos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Contactos')
CREATE TABLE Contactos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email VARCHAR(40) NOT NULL,
    Telefono INT NOT NULL
);
GO

-- Usuarios
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Usuarios')
CREATE TABLE Usuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NombreUsuario VARCHAR(50) NOT NULL,
    PasswordHash VARCHAR(100) NOT NULL,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    Activo BIT NOT NULL DEFAULT 1,
    IdContacto INT NOT NULL FOREIGN KEY REFERENCES Contactos(Id)
);
GO

-- Provincias
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Provincias')
CREATE TABLE Provincias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NombreProvincia VARCHAR(50) NOT NULL
);
GO

-- Ciudades
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Ciudades')
CREATE TABLE Ciudades (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NombreCiudad VARCHAR(50) NOT NULL,
    Cp INT NOT NULL,
    IdProvincia INT NOT NULL FOREIGN KEY REFERENCES Provincias(Id)
);
GO

-- Direcciones
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Direcciones')
CREATE TABLE Direcciones (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Calle VARCHAR(50) NOT NULL,
    Numero INT NOT NULL,
    Departamento VARCHAR(20),
    IdCiudad INT NOT NULL FOREIGN KEY REFERENCES Ciudades(Id)
);
GO

-- Categorias
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categorias')
CREATE TABLE Categorias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NombreCategoria VARCHAR(50) NOT NULL
);
GO

-- Productos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Productos')
CREATE TABLE Productos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NombreProducto VARCHAR(50) NOT NULL,
    Descripcion VARCHAR(100),
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Stock INT NOT NULL DEFAULT 0,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    Activo BIT NOT NULL DEFAULT 1,
    IdCategoria INT NOT NULL FOREIGN KEY REFERENCES Categorias(Id)
);
GO

-- MetodosPago
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MetodosPago')
CREATE TABLE MetodosPago (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    RecargoPorcentaje DECIMAL(5,2) NOT NULL DEFAULT 0,
    Activo BIT NOT NULL DEFAULT 1
);
GO

-- Ventas
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Ventas')
CREATE TABLE Ventas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NumeroVenta INT NOT NULL,
    Descuento INT NOT NULL DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL,
    IdMetodoPago INT NOT NULL FOREIGN KEY REFERENCES MetodosPago(Id),
    TipoEntrega VARCHAR(30) NOT NULL,
    Notas VARCHAR(200),
    Estado VARCHAR(20) NOT NULL DEFAULT 'Pendiente',
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    IdDireccion INT FOREIGN KEY REFERENCES Direcciones(Id),
    IdUsuario INT NOT NULL FOREIGN KEY REFERENCES Usuarios(Id)
);
GO

-- DetalleVentas
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DetallesVenta')
CREATE TABLE DetallesVenta (
    Id INT IDENTITY(1,1),
    IdVenta INT NOT NULL,
    IdProducto INT NOT NULL,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    SubTotal DECIMAL(18,2) NOT NULL,
    PRIMARY KEY (IdVenta, IdProducto),
    FOREIGN KEY (IdVenta) REFERENCES Ventas(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdProducto) REFERENCES Productos(Id)
);
GO

-- Clientes
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Clientes')
CREATE TABLE Clientes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Dni VARCHAR(15) NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Telefono VARCHAR(20) NOT NULL,
    Email VARCHAR(40),
    DireccionDefault VARCHAR(100),
    NumeroDefault INT,
    DepartamentoDefault VARCHAR(20),
    IdCiudad INT FOREIGN KEY REFERENCES Ciudades(Id),
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    Activo BIT NOT NULL DEFAULT 1
);
GO

-- AspNetUsers (Identity)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUsers')
BEGIN
    CREATE TABLE AspNetUsers (
        Id NVARCHAR(450) PRIMARY KEY,
        UserName NVARCHAR(256),
        NormalizedUserName NVARCHAR(256),
        Email NVARCHAR(256),
        NormalizedEmail NVARCHAR(256),
        EmailConfirmed BIT NOT NULL DEFAULT 0,
        PasswordHash NVARCHAR(MAX),
        SecurityStamp NVARCHAR(MAX),
        ConcurrencyStamp NVARCHAR(MAX),
        PhoneNumber NVARCHAR(MAX),
        PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
        TwoFactorEnabled BIT NOT NULL DEFAULT 0,
        LockoutEnd DATETIMEOFFSET,
        LockoutEnabled BIT NOT NULL DEFAULT 0,
        AccessFailedCount INT NOT NULL DEFAULT 0
    );
END
GO

-- AspNetRoles (Identity)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoles')
BEGIN
    CREATE TABLE AspNetRoles (
        Id NVARCHAR(450) PRIMARY KEY,
        Name NVARCHAR(256),
        NormalizedName NVARCHAR(256),
        ConcurrencyStamp NVARCHAR(MAX)
    );
END
GO

-- AspNetUserRoles (Identity)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserRoles')
BEGIN
    CREATE TABLE AspNetUserRoles (
        UserId NVARCHAR(450) NOT NULL,
        RoleId NVARCHAR(450) NOT NULL,
        PRIMARY KEY (UserId, RoleId),
        FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
        FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id)
    );
END
GO

-- AspNetUserClaims (Identity)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserClaims')
BEGIN
    CREATE TABLE AspNetUserClaims (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId NVARCHAR(450) NOT NULL FOREIGN KEY REFERENCES AspNetUsers(Id),
        ClaimType NVARCHAR(MAX),
        ClaimValue NVARCHAR(MAX)
    );
END
GO

-- AspNetUserLogins (Identity)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserLogins')
BEGIN
    CREATE TABLE AspNetUserLogins (
        LoginProvider NVARCHAR(450) NOT NULL,
        ProviderKey NVARCHAR(450) NOT NULL,
        ProviderDisplayName NVARCHAR(MAX),
        UserId NVARCHAR(450) NOT NULL FOREIGN KEY REFERENCES AspNetUsers(Id),
        PRIMARY KEY (LoginProvider, ProviderKey)
    );
END
GO

-- AspNetUserTokens (Identity)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserTokens')
BEGIN
    CREATE TABLE AspNetUserTokens (
        UserId NVARCHAR(450) NOT NULL,
        LoginProvider NVARCHAR(450) NOT NULL,
        Name NVARCHAR(450) NOT NULL,
        Value NVARCHAR(MAX),
        PRIMARY KEY (UserId, LoginProvider, Name),
        FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
    );
END
GO

-- AspNetRoleClaims (Identity)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoleClaims')
BEGIN
    CREATE TABLE AspNetRoleClaims (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RoleId NVARCHAR(450) NOT NULL FOREIGN KEY REFERENCES AspNetRoles(Id),
        ClaimType NVARCHAR(MAX),
        ClaimValue NVARCHAR(MAX)
    );
END
GO

-- =============================================
-- SEED DATA
-- =============================================

-- Métodos de pago
IF NOT EXISTS (SELECT 1 FROM MetodosPago)
BEGIN
    INSERT INTO MetodosPago (Nombre, RecargoPorcentaje, Activo) VALUES
        ('Efectivo', 0, 1),
        ('Tarjeta', 3, 1),
        ('Transferencia', 1.5, 1);
END
GO

-- Categorías
IF NOT EXISTS (SELECT 1 FROM Categorias)
BEGIN
    INSERT INTO Categorias (NombreCategoria) VALUES
        ('Herramientas Manuales'),
        ('Electricidad'),
        ('Fontanería'),
        ('Pintura'),
        ('Fijaciones');
END
GO

-- =============================================
-- TABLA DE AUDITORIA
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditoriaVenta')
CREATE TABLE AuditoriaVenta (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdVenta INT NOT NULL FOREIGN KEY REFERENCES Ventas(Id),
    EstadoAnterior VARCHAR(20),
    EstadoNuevo VARCHAR(20) NOT NULL,
    Usuario VARCHAR(100),
    FechaCambio DATETIME2 DEFAULT GETDATE()
);
GO

-- =============================================
-- TRIGGERS
-- =============================================

-- Trigger: auditoría de cambios de estado
CREATE OR ALTER TRIGGER trg_Audit_VentaEstado
ON Ventas AFTER UPDATE
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
END;
GO

-- Trigger: evitar modificar/cancelar ventas entregadas o canceladas
CREATE OR ALTER TRIGGER trg_PreventCancelarEntregada
ON Ventas AFTER UPDATE
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
END;
GO

-- =============================================
-- STORED PROCEDURES
-- =============================================

-- SP: Registrar venta
CREATE OR ALTER PROCEDURE sp_RegistrarVenta
    @NumeroVenta INT,
    @Descuento INT,
    @IdMetodoPago INT,
    @TipoEntrega VARCHAR(30),
    @Estado VARCHAR(20) = 'Pendiente',
    @Notas VARCHAR(200) = NULL,
    @IdDireccion INT = NULL,
    @IdUsuario INT,
    @Total DECIMAL(18,2) = 0
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @IdVenta INT, @ErrMsg NVARCHAR(4000);
    BEGIN TRY
        BEGIN TRANSACTION;
        INSERT INTO Ventas (NumeroVenta, Descuento, IdMetodoPago, TipoEntrega, Estado,
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
END;
GO

-- SP: Cancelar venta con devolución de stock
CREATE OR ALTER PROCEDURE sp_CancelarVenta
    @IdVenta INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrMsg NVARCHAR(4000);
    BEGIN TRY
        BEGIN TRANSACTION;
        UPDATE Productos SET Stock = Stock + dv.Cantidad
        FROM Productos p INNER JOIN DetallesVenta dv ON p.Id = dv.IdProducto
        WHERE dv.IdVenta = @IdVenta;
        DELETE FROM DetallesVenta WHERE IdVenta = @IdVenta;
        UPDATE Ventas SET Estado = 'Cancelada' WHERE Id = @IdVenta;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        SET @ErrMsg = ERROR_MESSAGE();
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW 50033, @ErrMsg, 1;
    END CATCH;
END;
GO

-- SP: Historial paginado de ventas
CREATE OR ALTER PROCEDURE sp_ObtenerHistorialVentas
    @Pagina INT = 1,
    @TamanoPagina INT = 10,
    @IdUsuario INT = NULL,
    @Estado VARCHAR(20) = NULL,
    @FechaDesde DATETIME2 = NULL,
    @FechaHasta DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@Pagina - 1) * @TamanoPagina;

    SELECT COUNT(*) AS Total FROM Ventas v
    WHERE (@IdUsuario IS NULL OR v.IdUsuario = @IdUsuario)
      AND (@Estado IS NULL OR v.Estado = @Estado)
      AND (@FechaDesde IS NULL OR v.FechaCreacion >= @FechaDesde)
      AND (@FechaHasta IS NULL OR v.FechaCreacion <= @FechaHasta);

    SELECT v.Id, v.NumeroVenta, v.Estado, v.Total,
        mp.Nombre AS NombreMetodoPago, v.FechaCreacion,
        (SELECT COUNT(*) FROM DetallesVenta dv WHERE dv.IdVenta = v.Id) AS CantidadItems
    FROM Ventas v
    LEFT JOIN MetodosPago mp ON v.IdMetodoPago = mp.Id
    WHERE (@IdUsuario IS NULL OR v.IdUsuario = @IdUsuario)
      AND (@Estado IS NULL OR v.Estado = @Estado)
      AND (@FechaDesde IS NULL OR v.FechaCreacion >= @FechaDesde)
      AND (@FechaHasta IS NULL OR v.FechaCreacion <= @FechaHasta)
    ORDER BY v.FechaCreacion DESC
    OFFSET @Offset ROWS FETCH NEXT @TamanoPagina ROWS ONLY;
END;
GO

-- SP: Estadísticas de ventas
CREATE OR ALTER PROCEDURE sp_ObtenerEstadisticasVentas
    @FechaDesde DATETIME2 = NULL,
    @FechaHasta DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        COUNT(*) AS TotalVentas,
        ISNULL(SUM(Total), 0) AS TotalFacturado,
        SUM(CASE WHEN Estado = 'Cancelada' THEN 1 ELSE 0 END) AS VentasCanceladas,
        SUM(CASE WHEN Estado = 'Pendiente' THEN 1 ELSE 0 END) AS VentasPendientes,
        SUM(CASE WHEN Estado = 'Entregada' THEN 1 ELSE 0 END) AS VentasEntregadas,
        @FechaDesde AS FechaDesde,
        @FechaHasta AS FechaHasta
    FROM Ventas
    WHERE (@FechaDesde IS NULL OR FechaCreacion >= @FechaDesde)
      AND (@FechaHasta IS NULL OR FechaCreacion <= @FechaHasta);
END;
GO

PRINT '=======================';
PRINT 'SCRIPT COMPLETO: OK';
PRINT '=======================';
GO
