-- ============================================================
--  SCRIPT COMPLETO FINAL — Sistema de Ventas
--  SQLite | Visual Studio 2022
--  Tablas confirmadas: Ventas, DetallesVenta, Productos,
--  Usuarios, Contactos, Direcciones, Ciudades, Provincias,
--  Categorias, Log_Ventas, Log_Stock
-- ============================================================
-- Eliminar triggers
DROP TRIGGER IF EXISTS trg_validar_stock;
DROP TRIGGER IF EXISTS trg_descontar_stock;
DROP TRIGGER IF EXISTS trg_recalcular_total_insert;
DROP TRIGGER IF EXISTS trg_recalcular_total_update;
DROP TRIGGER IF EXISTS trg_reponer_stock_delete;
DROP TRIGGER IF EXISTS trg_cancelar_venta_reponer_stock;
DROP TRIGGER IF EXISTS trg_auditoria_estado_venta;
DROP TRIGGER IF EXISTS trg_bloquear_venta_cerrada;
DROP TRIGGER IF EXISTS trg_email_unico_insert;
DROP TRIGGER IF EXISTS trg_email_unico_update;

-- Eliminar vistas
DROP VIEW IF EXISTS vista_venta_completa;
DROP VIEW IF EXISTS vista_ventas_por_usuario;
DROP VIEW IF EXISTS vista_productos_mas_vendidos;
DROP VIEW IF EXISTS vista_stock_bajo;
DROP VIEW IF EXISTS vista_auditoria_ventas;
DROP VIEW IF EXISTS vista_movimientos_stock;

-- Eliminar tablas de auditoría
DROP TABLE IF EXISTS Log_Ventas;
DROP TABLE IF EXISTS Log_Stock;

SELECT name, type FROM sqlite_master 
WHERE type IN ('trigger','view','table')
ORDER BY type, name;



PRAGMA foreign_keys = ON;


-- ============================================================
--  1. TABLAS DE AUDITORÍA
-- ============================================================

CREATE TABLE IF NOT EXISTS Log_Ventas (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    IdVenta         INTEGER NOT NULL,
    EstadoAnterior  TEXT,
    EstadoNuevo     TEXT,
    Fecha           TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS Log_Stock (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    IdProducto      INTEGER NOT NULL,
    Movimiento      TEXT    NOT NULL,
    Cantidad        INTEGER NOT NULL,
    StockAnterior   INTEGER NOT NULL,
    StockNuevo      INTEGER NOT NULL,
    IdVenta         INTEGER,
    Fecha           TEXT NOT NULL DEFAULT (datetime('now'))
);


-- ============================================================
--  2. TRIGGERS
-- ============================================================

-- ------------------------------------------------------------
-- TRG-01: Validar stock y producto activo ANTES de insertar
--         en DetallesVenta.
-- ------------------------------------------------------------
DROP TRIGGER IF EXISTS trg_validar_stock;
CREATE TRIGGER trg_validar_stock
BEFORE INSERT ON DetallesVenta
BEGIN
    SELECT CASE
        WHEN (SELECT Activo FROM Productos WHERE Id = NEW.IdProducto) = 0
            THEN RAISE(ABORT, 'El producto está inactivo y no puede venderse')
        WHEN (SELECT Stock FROM Productos WHERE Id = NEW.IdProducto) < NEW.Cantidad
            THEN RAISE(ABORT, 'Stock insuficiente para el producto seleccionado')
    END;
END;


-- ------------------------------------------------------------
-- TRG-02: Descontar stock DESPUÉS de insertar el detalle
--         y registrar en Log_Stock.
-- ------------------------------------------------------------
DROP TRIGGER IF EXISTS trg_descontar_stock;
CREATE TRIGGER trg_descontar_stock
AFTER INSERT ON DetallesVenta
BEGIN
    UPDATE Productos
    SET Stock = Stock - NEW.Cantidad
    WHERE Id = NEW.IdProducto;

    INSERT INTO Log_Stock (IdProducto, Movimiento, Cantidad,
                           StockAnterior, StockNuevo, IdVenta)
    VALUES (
        NEW.IdProducto,
        'DESCUENTO',
        NEW.Cantidad,
        (SELECT Stock + NEW.Cantidad FROM Productos WHERE Id = NEW.IdProducto),
        (SELECT Stock                FROM Productos WHERE Id = NEW.IdProducto),
        NEW.IdVenta
    );
END;


-- ------------------------------------------------------------
-- TRG-03: Recalcular Total de la Venta al INSERTAR un detalle.
--         Total = SUM(SubTotal) aplicando el Descuento (%).
-- ------------------------------------------------------------
DROP TRIGGER IF EXISTS trg_recalcular_total_insert;
CREATE TRIGGER trg_recalcular_total_insert
AFTER INSERT ON DetallesVenta
BEGIN
    UPDATE Ventas
    SET Total = (
        SELECT ROUND(
            SUM(dv.SubTotal) * (1.0 - v.Descuento / 100.0), 2
        )
        FROM DetallesVenta dv
        JOIN Ventas v ON v.Id = dv.IdVenta
        WHERE dv.IdVenta = NEW.IdVenta
    )
    WHERE Id = NEW.IdVenta;
END;


-- ------------------------------------------------------------
-- TRG-04: Recalcular Total al ACTUALIZAR un detalle.
-- ------------------------------------------------------------
DROP TRIGGER IF EXISTS trg_recalcular_total_update;
CREATE TRIGGER trg_recalcular_total_update
AFTER UPDATE ON DetallesVenta
BEGIN
    UPDATE Ventas
    SET Total = (
        SELECT ROUND(
            SUM(dv.SubTotal) * (1.0 - v.Descuento / 100.0), 2
        )
        FROM DetallesVenta dv
        JOIN Ventas v ON v.Id = dv.IdVenta
        WHERE dv.IdVenta = NEW.IdVenta
    )
    WHERE Id = NEW.IdVenta;
END;


-- ------------------------------------------------------------
-- TRG-05: Reponer stock al ELIMINAR un ítem del detalle.
-- ------------------------------------------------------------
DROP TRIGGER IF EXISTS trg_reponer_stock_delete;
CREATE TRIGGER trg_reponer_stock_delete
AFTER DELETE ON DetallesVenta
BEGIN
    UPDATE Productos
    SET Stock = Stock + OLD.Cantidad
    WHERE Id = OLD.IdProducto;

    INSERT INTO Log_Stock (IdProducto, Movimiento, Cantidad,
                           StockAnterior, StockNuevo, IdVenta)
    VALUES (
        OLD.IdProducto,
        'REPOSICION',
        OLD.Cantidad,
        (SELECT Stock - OLD.Cantidad FROM Productos WHERE Id = OLD.IdProducto),
        (SELECT Stock                FROM Productos WHERE Id = OLD.IdProducto),
        OLD.IdVenta
    );
END;


-- ------------------------------------------------------------
-- TRG-06: Al CANCELAR una venta reponer stock de todos
--         sus ítems automáticamente.
-- ------------------------------------------------------------
DROP TRIGGER IF EXISTS trg_cancelar_venta_reponer_stock;
CREATE TRIGGER trg_cancelar_venta_reponer_stock
AFTER UPDATE OF Estado ON Ventas
WHEN NEW.Estado = 'Cancelada' AND OLD.Estado != 'Cancelada'
BEGIN
    UPDATE Productos
    SET Stock = Stock + (
        SELECT dv.Cantidad
        FROM DetallesVenta dv
        WHERE dv.IdProducto = Productos.Id
          AND dv.IdVenta    = NEW.Id
    )
    WHERE Id IN (
        SELECT IdProducto FROM DetallesVenta WHERE IdVenta = NEW.Id
    );

    INSERT INTO Log_Stock (IdProducto, Movimiento, Cantidad,
                           StockAnterior, StockNuevo, IdVenta)
    SELECT
        dv.IdProducto,
        'REPOSICION',
        dv.Cantidad,
        (SELECT Stock - dv.Cantidad FROM Productos WHERE Id = dv.IdProducto),
        (SELECT Stock               FROM Productos WHERE Id = dv.IdProducto),
        NEW.Id
    FROM DetallesVenta dv
    WHERE dv.IdVenta = NEW.Id;
END;


-- ------------------------------------------------------------
-- TRG-07: Auditoría de cambios de Estado en Ventas.
-- ------------------------------------------------------------
DROP TRIGGER IF EXISTS trg_auditoria_estado_venta;
CREATE TRIGGER trg_auditoria_estado_venta
AFTER UPDATE OF Estado ON Ventas
WHEN OLD.Estado != NEW.Estado
BEGIN
    INSERT INTO Log_Ventas (IdVenta, EstadoAnterior, EstadoNuevo)
    VALUES (NEW.Id, OLD.Estado, NEW.Estado);
END;


-- ------------------------------------------------------------
-- TRG-08: Bloquear modificación de ventas Canceladas
--         o Entregadas.
-- ------------------------------------------------------------
DROP TRIGGER IF EXISTS trg_bloquear_venta_cerrada;
CREATE TRIGGER trg_bloquear_venta_cerrada
BEFORE UPDATE ON Ventas
WHEN OLD.Estado IN ('Cancelada', 'Entregada')
BEGIN
    SELECT RAISE(ABORT, 'No se puede modificar una venta Cancelada o Entregada');
END;


-- ------------------------------------------------------------
-- TRG-09: Validar email único en Contactos.
-- ------------------------------------------------------------
DROP TRIGGER IF EXISTS trg_email_unico_insert;
CREATE TRIGGER trg_email_unico_insert
BEFORE INSERT ON Contactos
BEGIN
    SELECT CASE
        WHEN (SELECT COUNT(*) FROM Contactos WHERE email = NEW.email) > 0
        THEN RAISE(ABORT, 'El email ya está registrado en el sistema')
    END;
END;

DROP TRIGGER IF EXISTS trg_email_unico_update;
CREATE TRIGGER trg_email_unico_update
BEFORE UPDATE OF email ON Contactos
BEGIN
    SELECT CASE
        WHEN (SELECT COUNT(*) FROM Contactos
              WHERE email = NEW.email AND Id != OLD.Id) > 0
        THEN RAISE(ABORT, 'El email ya pertenece a otro contacto')
    END;
END;


-- ============================================================
--  3. VISTAS
-- ============================================================

DROP VIEW IF EXISTS vista_venta_completa;
CREATE VIEW vista_venta_completa AS
SELECT
    v.Id              AS IdVenta,
    v.NumeroVenta,
    v.Estado,
    v.MetodoPago,
    v.TipoEntrega,
    v.Descuento,
    v.Total,
    v.FechaCreacion,
    v.Notas,
    u.NombreUsuario,
    d.Calle || ' ' || d.Numero  AS Direccion,
    p.Id              AS IdProducto,
    p.NombreProducto,
    dv.Cantidad,
    dv.PrecioUnitario,
    dv.SubTotal
FROM Ventas v
JOIN Usuarios u       ON v.IdUsuario   = u.Id
JOIN Direcciones d    ON v.IdDireccion = d.Id
JOIN DetallesVenta dv ON v.Id          = dv.IdVenta
JOIN Productos p      ON dv.IdProducto = p.Id;


DROP VIEW IF EXISTS vista_ventas_por_usuario;
CREATE VIEW vista_ventas_por_usuario AS
SELECT
    u.Id              AS IdUsuario,
    u.NombreUsuario,
    COUNT(v.Id)                                              AS CantidadVentas,
    ROUND(SUM(v.Total), 2)                                   AS TotalFacturado,
    SUM(CASE WHEN v.Estado = 'Cancelada' THEN 1 ELSE 0 END) AS Canceladas,
    MAX(v.FechaCreacion)                                     AS UltimaVenta
FROM Usuarios u
LEFT JOIN Ventas v ON u.Id = v.IdUsuario
GROUP BY u.Id, u.NombreUsuario;


DROP VIEW IF EXISTS vista_productos_mas_vendidos;
CREATE VIEW vista_productos_mas_vendidos AS
SELECT
    p.Id              AS IdProducto,
    p.NombreProducto,
    SUM(dv.Cantidad)            AS UnidadesVendidas,
    ROUND(SUM(dv.SubTotal), 2)  AS IngresosGenerados,
    COUNT(DISTINCT dv.IdVenta)  AS ApareceEnVentas
FROM Productos p
JOIN DetallesVenta dv ON p.Id       = dv.IdProducto
JOIN Ventas v         ON dv.IdVenta = v.Id
WHERE v.Estado != 'Cancelada'
GROUP BY p.Id, p.NombreProducto
ORDER BY UnidadesVendidas DESC;


DROP VIEW IF EXISTS vista_stock_bajo;
CREATE VIEW vista_stock_bajo AS
SELECT
    p.Id              AS IdProducto,
    p.NombreProducto,
    p.Stock,
    p.PrecioUnitario
FROM Productos p
WHERE p.Activo = 1
  AND p.Stock < 5
ORDER BY p.Stock ASC;


DROP VIEW IF EXISTS vista_auditoria_ventas;
CREATE VIEW vista_auditoria_ventas AS
SELECT
    lv.Id,
    lv.IdVenta,
    v.NumeroVenta,
    lv.EstadoAnterior,
    lv.EstadoNuevo,
    lv.Fecha
FROM Log_Ventas lv
JOIN Ventas v ON lv.IdVenta = v.Id
ORDER BY lv.Fecha DESC;


DROP VIEW IF EXISTS vista_movimientos_stock;
CREATE VIEW vista_movimientos_stock AS
SELECT
    ls.Id,
    p.NombreProducto,
    ls.Movimiento,
    ls.Cantidad,
    ls.StockAnterior,
    ls.StockNuevo,
    ls.IdVenta,
    ls.Fecha
FROM Log_Stock ls
JOIN Productos p ON ls.IdProducto = p.Id
ORDER BY ls.Fecha DESC;


-- ============================================================
--  4. TRANSACCIONES DE EJEMPLO
-- ============================================================

-- ------------------------------------------------------------
-- TX-01: Registrar una venta completa
--        Descomentá, ajustá los IDs y ejecutá.
-- ------------------------------------------------------------

BEGIN TRANSACTION;

    INSERT INTO Ventas (
        NumeroVenta, Descuento, MetodoPago,
        TipoEntrega, Notas, Estado, Total,
        FechaCreacion, IdDireccion, IdUsuario
    ) VALUES (
        'V-' || strftime('%Y%m%d%H%M%S','now'),
        10,
        'Transferencia',
        'Envío',
        'Dejar en portería',
        'Pendiente',
        0,
        datetime('now'),
        1,   -- IdDireccion (debe existir en Direcciones)
        1    -- IdUsuario   (debe existir en Usuarios)
    );

    INSERT INTO DetallesVenta (IdVenta, IdProducto, Cantidad, SubTotal, PrecioUnitario)
    VALUES
        ((SELECT MAX(Id) FROM Ventas), 1, 2,
            2 * (SELECT PrecioUnitario FROM Productos WHERE Id = 1),
            (SELECT PrecioUnitario FROM Productos WHERE Id = 1)),
        ((SELECT MAX(Id) FROM Ventas), 2, 1,
            1 * (SELECT PrecioUnitario FROM Productos WHERE Id = 2),
            (SELECT PrecioUnitario FROM Productos WHERE Id = 2));

COMMIT;


-- ------------------------------------------------------------
-- TX-02: Cancelar una venta
--        TRG-06 repone stock y loggea, TRG-07 audita el cambio.
-- ------------------------------------------------------------

-- Primero verificar que se puede cancelar:
SELECT Id, Estado FROM Ventas WHERE Id = 1;

-- Si Estado no es Cancelada ni Entregada, ejecutar:
BEGIN TRANSACTION;
    UPDATE Ventas SET Estado = 'Cancelada' WHERE Id = 2;
COMMIT;

SELECT Id, NumeroVenta, Estado FROM Ventas; --ver que ids quedan libres
-- Verificar resultado:
SELECT * FROM vista_auditoria_ventas;
SELECT * FROM vista_movimientos_stock;


-- ------------------------------------------------------------
-- TX-03: Dar de alta usuario con contacto
-- ------------------------------------------------------------

BEGIN TRANSACTION;

    INSERT INTO Contactos (email, Telefono)
    VALUES ('nuevo@mail.com', '3794999888');

    INSERT INTO Usuarios (NombreUsuario, PasswordHash, FechaCreacion, Activo, IdContacto)
    VALUES ('nuevousuario', '1234', datetime('now'), 1, last_insert_rowid());

COMMIT;



-- ------------------------------------------------------------
-- TX-04: Ajuste manual de stock (reposición de proveedor)
-- ------------------------------------------------------------

BEGIN TRANSACTION;

    UPDATE Productos
    SET Stock = Stock + 50
    WHERE Id = 1;

    INSERT INTO Log_Stock (IdProducto, Movimiento, Cantidad,
                           StockAnterior, StockNuevo)
    VALUES (
        1,
        'REPOSICION',
        50,
        (SELECT Stock - 50 FROM Productos WHERE Id = 1),
        (SELECT Stock       FROM Productos WHERE Id = 1)
    );

COMMIT;



-- ------------------------------------------------------------
-- TX-05: Actualizar precio y recalcular detalles pendientes
-- ------------------------------------------------------------

BEGIN TRANSACTION;

    UPDATE Productos
    SET PrecioUnitario = 9000.00
    WHERE Id = 1;

    UPDATE DetallesVenta
    SET SubTotal       = Cantidad * 9000.00,
        PrecioUnitario = 9000.00
    WHERE IdProducto = 1
      AND IdVenta IN (
          SELECT Id FROM Ventas WHERE Estado = 'Pendiente'
      );

COMMIT;


-- ============================================================
--  5. CONSULTAS DE VERIFICACIÓN
-- ============================================================

-- Ver todas las ventas completas
SELECT * FROM vista_venta_completa;

-- Ventas por usuario
SELECT * FROM vista_ventas_por_usuario;

-- Productos más vendidos
SELECT * FROM vista_productos_mas_vendidos;

-- Stock bajo
SELECT * FROM vista_stock_bajo;

-- Auditoría de ventas
SELECT * FROM vista_auditoria_ventas;

-- Movimientos de stock
SELECT * FROM vista_movimientos_stock;