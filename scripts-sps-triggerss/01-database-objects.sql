-- ============================================================================
-- SCRIPT COMPLETO: Creación de objetos de base de datos SISIE
-- ============================================================================
-- Este script contiene todos los objetos programables (SPs, triggers, tablas
-- auxiliares) que se crean automáticamente al iniciar la aplicación.
--
-- NOTA: Para crear la BD desde cero, ejecutar primero:
--   00-schema-completo.sql (crea BD + tablas + seed + SPs + triggers)
-- Este script es solo para actualizar SPs/triggers si ya tenés la BD.
--
-- ============================================================================

SET NOCOUNT ON;
GO

-- ============================================================================
-- TABLA: AuditoriaVenta
-- ============================================================================
-- Creada automáticamente si no existe. Registra cambios de estado en Ventas.
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

-- ============================================================================
-- TABLA: MetodoPago (solo si EF no la creó)
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MetodosPago')
CREATE TABLE MetodosPago (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    RecargoPorcentaje DECIMAL(5,2) NOT NULL DEFAULT 0,
    Activo BIT NOT NULL DEFAULT 1
);
GO

-- Seed de métodos de pago (solo si están vacíos)
IF NOT EXISTS (SELECT 1 FROM MetodosPago)
BEGIN
    INSERT INTO MetodosPago (Nombre, RecargoPorcentaje, Activo) VALUES
        ('Efectivo', 0, 1),
        ('Tarjeta', 3, 1),
        ('Transferencia', 1.5, 1);
END;
GO

-- ============================================================================
-- TRIGGER: trg_Audit_VentaEstado
-- ============================================================================
-- Se dispara AFTER UPDATE en Ventas.
-- Cada vez que cambia el Estado, inserta un registro en AuditoriaVenta.
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

-- ============================================================================
-- TRIGGER: trg_PreventCancelarEntregada
-- ============================================================================
-- Se dispara AFTER UPDATE en Ventas.
-- Impide modificar una venta Entregada o reactivar una Cancelada.
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

-- ============================================================================
-- STORED PROCEDURE: sp_RegistrarVenta
-- ============================================================================
-- Inserta una venta con manejo transaccional.
-- Devuelve el ID de la venta creada via SELECT.
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

-- ============================================================================
-- STORED PROCEDURE: sp_RegistrarDetalleVenta
-- ============================================================================
-- Inserta un detalle de venta con validación de stock.
-- NO descuenta stock (el descuento lo hace ActualizarStockAsync desde el service).
CREATE OR ALTER PROCEDURE sp_RegistrarDetalleVenta
    @IdVenta INT,
    @IdProducto INT,
    @Cantidad INT,
    @PrecioUnitario DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrMsg NVARCHAR(4000), @StockActual INT;
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @StockActual = Stock FROM Productos WHERE Id = @IdProducto;
        IF @StockActual IS NULL THROW 50020, 'Producto no encontrado.', 1;
        IF @StockActual < @Cantidad
        BEGIN
            SET @ErrMsg = 'Stock insuficiente. Disp: ' + CAST(@StockActual AS VARCHAR)
                + ', req: ' + CAST(@Cantidad AS VARCHAR);
            THROW 50021, @ErrMsg, 1;
        END
        INSERT INTO DetallesVenta (IdVenta, IdProducto, Cantidad, PrecioUnitario, SubTotal)
        VALUES (@IdVenta, @IdProducto, @Cantidad, @PrecioUnitario, @Cantidad * @PrecioUnitario);
        SELECT SCOPE_IDENTITY() AS Id;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        SET @ErrMsg = ERROR_MESSAGE();
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW 50022, @ErrMsg, 1;
    END CATCH;
END;
GO

-- ============================================================================
-- STORED PROCEDURE: sp_CancelarVenta
-- ============================================================================
-- Cancela una venta y restaura el stock de cada producto.
-- Valida que la venta exista, no esté cancelada ni entregada.
CREATE OR ALTER PROCEDURE sp_CancelarVenta
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
        FROM Productos p INNER JOIN DetallesVenta dv ON p.Id = dv.IdProducto
        WHERE dv.IdVenta = @IdVenta;
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

-- ============================================================================
-- STORED PROCEDURE: sp_ObtenerHistorialVentas
-- ============================================================================
-- Devuelve historial paginado con filtros opcionales.
-- Retorna dos result sets:
--   1. Total de registros (para paginación)
--   2. Items de la página actual
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

    -- Primer result set: Total de registros
    SELECT COUNT(*) AS Total FROM Ventas v
    WHERE (@IdUsuario IS NULL OR v.IdUsuario = @IdUsuario)
      AND (@Estado IS NULL OR v.Estado = @Estado)
      AND (@FechaDesde IS NULL OR v.FechaCreacion >= @FechaDesde)
      AND (@FechaHasta IS NULL OR v.FechaCreacion <= @FechaHasta);

    -- Segundo result set: Items paginados
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

-- ============================================================================
-- STORED PROCEDURE: sp_ObtenerEstadisticasVentas
-- ============================================================================
-- Devuelve estadísticas agregadas de ventas con filtro de fechas opcional.
CREATE OR ALTER PROCEDURE sp_ObtenerEstadisticasVentas
    @FechaDesde DATETIME2 = NULL,
    @FechaHasta DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) AS TotalVentas,
           ISNULL(SUM(Total), 0) AS TotalFacturado,
           COUNT(CASE WHEN Estado = 'Cancelada' THEN 1 END) AS VentasCanceladas,
           COUNT(CASE WHEN Estado = 'Pendiente' THEN 1 END) AS VentasPendientes,
           COUNT(CASE WHEN Estado = 'Entregada' THEN 1 END) AS VentasEntregadas,
           @FechaDesde AS FechaDesde,
           @FechaHasta AS FechaHasta
    FROM Ventas
    WHERE (@FechaDesde IS NULL OR FechaCreacion >= @FechaDesde)
      AND (@FechaHasta IS NULL OR FechaCreacion <= @FechaHasta);
END;
GO

PRINT '✅ Todos los objetos se crearon correctamente.';
GO
