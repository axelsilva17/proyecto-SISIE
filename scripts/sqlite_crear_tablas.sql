-- ============================================
-- Script SQL para crear las tablas del proyecto SISIE
-- Base de datos: SQLite
-- ============================================

-- Tabla: Categorias
CREATE TABLE IF NOT EXISTS Categorias (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    NombreCategoria TEXT NOT NULL UNIQUE
);

-- Tabla: Productos
CREATE TABLE IF NOT EXISTS Productos (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    NombreProducto TEXT NOT NULL,
    Descripcion TEXT,
    PrecioUnitario REAL NOT NULL,
    Stock INTEGER NOT NULL,
    FechaCreacion TEXT NOT NULL,
    Activo INTEGER NOT NULL DEFAULT 1,
    IdCategoria INTEGER NOT NULL,
    FOREIGN KEY (IdCategoria) REFERENCES Categorias (Id)
);

-- Tabla: Contactos
CREATE TABLE IF NOT EXISTS Contactos (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Email TEXT NOT NULL,
    Telefono INTEGER NOT NULL
);

-- Tabla: Usuarios
CREATE TABLE IF NOT EXISTS Usuarios (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    NombreUsuario TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    FechaCreacion TEXT NOT NULL,
    Activo INTEGER NOT NULL DEFAULT 1,
    IdContacto INTEGER NOT NULL,
    FOREIGN KEY (IdContacto) REFERENCES Contactos (Id)
);

-- Tabla: Provincias
CREATE TABLE IF NOT EXISTS Provincias (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    NombreProvincia TEXT NOT NULL UNIQUE
);

-- Tabla: Ciudades
CREATE TABLE IF NOT EXISTS Ciudades (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    NombreCiudad TEXT NOT NULL,
    Cp INTEGER NOT NULL,
    IdProvincia INTEGER NOT NULL,
    FOREIGN KEY (IdProvincia) REFERENCES Provincias (Id)
);

-- Tabla: Direcciones
CREATE TABLE IF NOT EXISTS Direcciones (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Calle TEXT NOT NULL,
    Numero INTEGER NOT NULL,
    IdCiudad INTEGER NOT NULL,
    FOREIGN KEY (IdCiudad) REFERENCES Ciudades (Id)
);

-- Tabla: Ventas
CREATE TABLE IF NOT EXISTS Ventas (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    NumeroVenta INTEGER NOT NULL,
    Descuento INTEGER NOT NULL DEFAULT 0,
    Total REAL NOT NULL,
    MetodoPago TEXT NOT NULL,
    TipoEntrega TEXT NOT NULL,
    Notas TEXT,
    Estado TEXT NOT NULL DEFAULT 'pendiente',
    FechaCreacion TEXT NOT NULL,
    IdDireccion INTEGER,
    IdUsuario INTEGER NOT NULL,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios (Id),
    FOREIGN KEY (IdDireccion) REFERENCES Direcciones (Id)
);

-- Tabla: DetallesVenta (FK Compuesta - Primary Key compuesto)
CREATE TABLE IF NOT EXISTS DetallesVenta (
    IdVenta INTEGER NOT NULL,
    IdProducto INTEGER NOT NULL,
    SubTotal REAL NOT NULL,
    Cantidad INTEGER NOT NULL,
    PrecioUnitario REAL NOT NULL,
    PRIMARY KEY (IdVenta, IdProducto),
    FOREIGN KEY (IdVenta) REFERENCES Ventas (Id),
    FOREIGN KEY (IdProducto) REFERENCES Productos (Id)
);

-- ============================================
-- Datos de prueba (seed)
-- ============================================

-- Categorías de ejemplo
INSERT INTO Categorias (NombreCategoria) VALUES 
    ('Herramientas'),
    ('Tornillos'),
    ('Pinturas'),
    ('Electricidad'),
    ('Fontaneria');

-- Productos de ejemplo
INSERT INTO Productos (NombreProducto, Descripcion, PrecioUnitario, Stock, FechaCreacion, Activo, IdCategoria) VALUES
    ('Martillo', 'Martillo de carpintero', 1500.00, 50, '2024-01-01', 1, 1),
    ('Destornillador', 'Juego de destornilladores', 800.00, 100, '2024-01-01', 1, 1),
    ('Tornillo 5mm', 'Caja de 50 unidades', 250.00, 200, '2024-01-01', 1, 2),
    ('Pintura blanca', 'Latex 4 litros', 3500.00, 30, '2024-01-01', 1, 3),
    ('Cable 2.5mm', 'Rollo de 100 metros', 2800.00, 20, '2024-01-01', 1, 4);