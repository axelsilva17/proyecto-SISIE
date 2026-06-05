using Moq;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Implementations;
using proyecto_SISIE.Services.Interfaces;
using proyecto_SISIE.Services.Strategy;

namespace proyecto_SISIE.Tests;

public class VentaServiceTests
{
    private readonly Mock<IVentaRepositorio> _ventaRepositorioMock;
    private readonly Mock<IProductoRepositorio> _productoRepositorioMock;
    private readonly Mock<IProductoService> _productoServiceMock;
    private readonly Mock<IClienteService> _clienteServiceMock;
    private readonly Mock<IValidadorVenta> _validadorMock;
    private readonly ProcesadorPago _procesadorPago;
    private readonly VentaService _service;

    public VentaServiceTests()
    {
        _ventaRepositorioMock = new Mock<IVentaRepositorio>();
        _productoRepositorioMock = new Mock<IProductoRepositorio>();
        _productoServiceMock = new Mock<IProductoService>();
        _clienteServiceMock = new Mock<IClienteService>();
        _validadorMock = new Mock<IValidadorVenta>();

        var mockEstrategia = new Mock<IMetodoPagoStrategy>();
        mockEstrategia.Setup(e => e.MetodoPago).Returns("Efectivo");
        mockEstrategia.Setup(e => e.CalcularTotal(It.IsAny<decimal>(), It.IsAny<int>()))
            .Returns<decimal, int>((subtotal, descuento) =>
            {
                var desc = (decimal)descuento / 100m;
                return Math.Round(subtotal * (1m - desc), 2);
            });

        _procesadorPago = new ProcesadorPago(new[] { mockEstrategia.Object });

        _service = new VentaService(
            _ventaRepositorioMock.Object,
            _productoRepositorioMock.Object,
            _productoServiceMock.Object,
            _clienteServiceMock.Object,
            _validadorMock.Object,
            _procesadorPago);
    }

    private Venta CrearVentaCompleta(int id, int idUsuario, List<VentaDetalleDTO> detallesDto)
    {
        var detalles = detallesDto.Select(d => new DetalleVenta
        {
            IdVenta = id,
            IdProducto = d.IdProducto,
            Cantidad = d.Cantidad,
            PrecioUnitario = 100m,
            SubTotal = d.Cantidad * 100m,
            Producto = new Producto
            {
                Id = d.IdProducto,
                NombreProducto = $"Producto {d.IdProducto}",
                PrecioUnitario = 100m,
                Stock = 50
            }
        }).ToList();

        return new Venta
        {
            Id = id,
            NumeroVenta = 1001,
            Descuento = 0,
            Total = detalles.Sum(d => d.SubTotal),
            MetodoPago = "Efectivo",
            TipoEntrega = "Mostrador",
            Estado = "Pendiente",
            FechaCreacion = DateTime.Now,
            IdUsuario = idUsuario,
            Usuario = new Usuario
            {
                Id = idUsuario,
                NombreUsuario = "testuser",
                Activo = true
            },
            Detalles = detalles
        };
    }

    [Fact]
    public async Task RegistrarVenta_DatosValidos_RetornaVentaDTO()
    {
        var idUsuario = 1;
        var detalles = new List<VentaDetalleDTO>
        {
            new() { IdProducto = 1, Cantidad = 2 }
        };

        var dto = new VentaCreateDTO
        {
            MetodoPago = "Efectivo",
            Detalles = detalles,
            EsEnvio = false
        };

        var ventaCreada = CrearVentaCompleta(1, idUsuario, detalles);

        _validadorMock
            .Setup(v => v.ValidarDatosVentaCreate(dto, idUsuario))
            .ReturnsAsync(new List<string>());

        _ventaRepositorioMock
            .Setup(r => r.CrearAsync(It.IsAny<Venta>()))
            .ReturnsAsync(ventaCreada);

        _productoServiceMock
            .Setup(p => p.VerificarStockProductoAsync(1, 2))
            .ReturnsAsync(new StockVerificacionDTO
            {
                IdProducto = 1,
                NombreProducto = "Producto 1",
                StockDisponible = 50,
                HayStock = true,
                Mensaje = "Stock disponible"
            });

        _productoRepositorioMock
            .Setup(r => r.ObtenerPorIdCrudoAsync(1))
            .ReturnsAsync(new Producto { Id = 1, NombreProducto = "Producto 1", PrecioUnitario = 100m, Stock = 50 });

        _productoServiceMock
            .Setup(p => p.ActualizarStockAsync(1, 2))
            .ReturnsAsync(true);

        _ventaRepositorioMock
            .Setup(r => r.ActualizarAsync(It.IsAny<Venta>()))
            .ReturnsAsync(ventaCreada);

        _ventaRepositorioMock
            .Setup(r => r.ObtenerPorIdConTodoAsync(1))
            .ReturnsAsync(ventaCreada);

        _productoServiceMock
            .Setup(p => p.VerificarStockProductoAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new StockVerificacionDTO { HayStock = true, Mensaje = "Stock disponible" });

        var result = await _service.RegistrarVentaAsync(idUsuario, dto);

        Assert.NotNull(result);
        Assert.Equal("Efectivo", result.MetodoPago);
        Assert.Equal("Mostrador", result.TipoEntrega);
        Assert.Equal("Pendiente", result.Estado);
        Assert.True(result.Total > 0);
        Assert.Single(result.Detalles);

        _ventaRepositorioMock.Verify(r => r.CrearAsync(It.IsAny<Venta>()), Times.Once);
        _ventaRepositorioMock.Verify(r => r.ActualizarAsync(It.IsAny<Venta>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarVenta_StockInsuficiente_LanzaExcepcion()
    {
        var idUsuario = 1;
        var detalles = new List<VentaDetalleDTO>
        {
            new() { IdProducto = 1, Cantidad = 999 }
        };

        var dto = new VentaCreateDTO
        {
            MetodoPago = "Efectivo",
            Detalles = detalles,
            EsEnvio = false
        };

        var errores = new List<string>
        {
            "Stock insuficiente para 'Producto 1'. Disponible: 10"
        };

        _validadorMock
            .Setup(v => v.ValidarDatosVentaCreate(dto, idUsuario))
            .ReturnsAsync(errores);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegistrarVentaAsync(idUsuario, dto));

        Assert.Contains("Stock insuficiente", ex.Message);

        _ventaRepositorioMock.Verify(r => r.CrearAsync(It.IsAny<Venta>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarVenta_ProductoInexistente_LanzaExcepcion()
    {
        var idUsuario = 1;
        var dto = new VentaCreateDTO
        {
            MetodoPago = "Efectivo",
            Detalles = new List<VentaDetalleDTO>
            {
                new() { IdProducto = 999, Cantidad = 1 }
            },
            EsEnvio = false
        };

        var errores = new List<string>
        {
            "El producto con ID 999 no existe"
        };

        _validadorMock
            .Setup(v => v.ValidarDatosVentaCreate(dto, idUsuario))
            .ReturnsAsync(errores);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegistrarVentaAsync(idUsuario, dto));

        Assert.Contains("El producto con ID 999 no existe", ex.Message);

        _ventaRepositorioMock.Verify(r => r.CrearAsync(It.IsAny<Venta>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarVenta_ListaProductosVacia_RetornaError()
    {
        var idUsuario = 1;
        var dto = new VentaCreateDTO
        {
            MetodoPago = "Efectivo",
            Detalles = new List<VentaDetalleDTO>(),
            EsEnvio = false
        };

        var errores = new List<string> { "Debe incluir al menos un producto" };

        _validadorMock
            .Setup(v => v.ValidarDatosVentaCreate(dto, idUsuario))
            .ReturnsAsync(errores);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegistrarVentaAsync(idUsuario, dto));

        Assert.Contains("Debe incluir al menos un producto", ex.Message);

        _ventaRepositorioMock.Verify(r => r.CrearAsync(It.IsAny<Venta>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarVenta_MetodoPagoInvalido_RetornaError()
    {
        var idUsuario = 1;
        var dto = new VentaCreateDTO
        {
            MetodoPago = "Cripto",
            Detalles = new List<VentaDetalleDTO>
            {
                new() { IdProducto = 1, Cantidad = 2 }
            },
            EsEnvio = false
        };

        _validadorMock
            .Setup(v => v.ValidarDatosVentaCreate(dto, idUsuario))
            .ReturnsAsync(new List<string>());

        _ventaRepositorioMock
            .Setup(r => r.CrearAsync(It.IsAny<Venta>()))
            .ReturnsAsync((Venta v) => v);

        _productoServiceMock
            .Setup(p => p.VerificarStockProductoAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new StockVerificacionDTO { HayStock = true, Mensaje = "Stock disponible" });

        _productoRepositorioMock
            .Setup(r => r.ObtenerPorIdCrudoAsync(It.IsAny<int>()))
            .ReturnsAsync(new Producto { Id = 1, NombreProducto = "Producto 1", PrecioUnitario = 100m, Stock = 50 });

        _productoServiceMock
            .Setup(p => p.ActualizarStockAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegistrarVentaAsync(idUsuario, dto));

        Assert.Contains("No hay una estrategia registrada para el método de pago", ex.Message);

        _ventaRepositorioMock.Verify(r => r.CrearAsync(It.IsAny<Venta>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarVenta_VerificaStockActualizado_PostCondicion()
    {
        var idUsuario = 1;
        var detalles = new List<VentaDetalleDTO>
        {
            new() { IdProducto = 1, Cantidad = 2 },
            new() { IdProducto = 2, Cantidad = 3 }
        };

        var dto = new VentaCreateDTO
        {
            MetodoPago = "Efectivo",
            Detalles = detalles,
            EsEnvio = false
        };

        var ventaCreada = CrearVentaCompleta(1, idUsuario, detalles);

        _validadorMock
            .Setup(v => v.ValidarDatosVentaCreate(dto, idUsuario))
            .ReturnsAsync(new List<string>());

        _ventaRepositorioMock
            .Setup(r => r.CrearAsync(It.IsAny<Venta>()))
            .ReturnsAsync(ventaCreada);

        _productoServiceMock
            .Setup(p => p.VerificarStockProductoAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new StockVerificacionDTO { HayStock = true, Mensaje = "Stock disponible" });

        _productoRepositorioMock
            .Setup(r => r.ObtenerPorIdCrudoAsync(It.IsAny<int>()))
            .ReturnsAsync(new Producto { Id = 1, NombreProducto = "Producto 1", PrecioUnitario = 100m, Stock = 50 });

        _productoServiceMock
            .Setup(p => p.ActualizarStockAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        _ventaRepositorioMock
            .Setup(r => r.ActualizarAsync(It.IsAny<Venta>()))
            .ReturnsAsync(ventaCreada);

        _ventaRepositorioMock
            .Setup(r => r.ObtenerPorIdConTodoAsync(1))
            .ReturnsAsync(ventaCreada);

        await _service.RegistrarVentaAsync(idUsuario, dto);

        _productoServiceMock.Verify(
            p => p.ActualizarStockAsync(It.IsAny<int>(), It.IsAny<int>()),
            Times.Exactly(2));

        _productoServiceMock.Verify(
            p => p.ActualizarStockAsync(1, 2), Times.Once);

        _productoServiceMock.Verify(
            p => p.ActualizarStockAsync(2, 3), Times.Once);
    }
}
