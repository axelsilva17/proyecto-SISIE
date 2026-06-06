using Moq;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Implementations;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Tests;

public class ProductoServiceTests
{
    private readonly Mock<IProductoRepositorio> _repositorioMock;
    private readonly Mock<IValidadorProducto> _validadorMock;
    private readonly Mock<IValidadorVenta> _validadorVentaMock;
    private readonly ProductoService _service;

    public ProductoServiceTests()
    {
        _repositorioMock = new Mock<IProductoRepositorio>();
        _validadorMock = new Mock<IValidadorProducto>();
        _validadorVentaMock = new Mock<IValidadorVenta>();
        _service = new ProductoService(
            _repositorioMock.Object,
            _validadorMock.Object,
            _validadorVentaMock.Object);
    }

    [Fact]
    public async Task RegistrarProducto_DatosValidos_RetornaProductoDTO()
    {
        var dto = new ProductoCreateDTO
        {
            NombreProducto = "Martillo",
            Descripcion = "Martillo 20 oz",
            PrecioUnitario = 1500m,
            Stock = 10,
            IdCategoria = 1
        };

        var categoria = new Categoria { Id = 1, NombreCategoria = "Herramientas" };
        var producto = new Producto
        {
            Id = 1,
            NombreProducto = "Martillo",
            Descripcion = "Martillo 20 oz",
            PrecioUnitario = 1500m,
            Stock = 10,
            IdCategoria = 1,
            Categoria = categoria,
            FechaCreacion = DateTime.Now,
            Activo = true
        };

        _validadorMock
            .Setup(v => v.ValidaProducto(dto, null))
            .ReturnsAsync(new List<string>());

        _repositorioMock
            .Setup(r => r.InsertarProductoAsync(It.IsAny<Producto>()))
            .ReturnsAsync(producto);

        var result = await _service.CrearAsyncProducto(dto);

        Assert.NotNull(result);
        Assert.Equal("Martillo", result.NombreProducto);
        Assert.Equal("Martillo 20 oz", result.Descripcion);
        Assert.Equal(1500m, result.PrecioUnitario);
        Assert.Equal(10, result.Stock);
        Assert.Equal(1, result.IdCategoria);
        Assert.Equal("Herramientas", result.NombreCategoria);
        Assert.True(result.Activo);

        _repositorioMock.Verify(r => r.InsertarProductoAsync(It.IsAny<Producto>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarProducto_NombreDuplicado_LanzaExcepcion()
    {
        var dto = new ProductoCreateDTO
        {
            NombreProducto = "Martillo",
            Descripcion = "Martillo 20 oz",
            PrecioUnitario = 1500m,
            Stock = 10,
            IdCategoria = 1
        };

        var errores = new List<string> { "Ya existe un producto con ese nombre" };

        _validadorMock
            .Setup(v => v.ValidaProducto(dto, null))
            .ReturnsAsync(errores);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CrearAsyncProducto(dto));

        Assert.Contains("Ya existe un producto con ese nombre", ex.Message);

        _repositorioMock.Verify(r => r.InsertarProductoAsync(It.IsAny<Producto>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(-1)]
    public async Task RegistrarProducto_PrecioInvalido_RetornaError(decimal precioInvalido)
    {
        var dto = new ProductoCreateDTO
        {
            NombreProducto = "Martillo",
            Descripcion = "Test",
            PrecioUnitario = precioInvalido,
            Stock = 10,
            IdCategoria = 1
        };

        var errores = new List<string> { "El precio debe ser mayor a 0" };

        _validadorMock
            .Setup(v => v.ValidaProducto(
                It.Is<ProductoCreateDTO>(p => p.PrecioUnitario <= 0), null))
            .ReturnsAsync(errores);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CrearAsyncProducto(dto));

        Assert.Contains("El precio debe ser mayor a 0", ex.Message);

        _repositorioMock.Verify(r => r.InsertarProductoAsync(It.IsAny<Producto>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task RegistrarProducto_StockInvalido_RetornaError(int stockInvalido)
    {
        var dto = new ProductoCreateDTO
        {
            NombreProducto = "Martillo",
            Descripcion = "Test",
            PrecioUnitario = 1500m,
            Stock = stockInvalido,
            IdCategoria = 1
        };

        var errores = new List<string> { "El stock debe ser mayor a 0" };

        _validadorMock
            .Setup(v => v.ValidaProducto(
                It.Is<ProductoCreateDTO>(p => p.Stock <= 0), null))
            .ReturnsAsync(errores);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CrearAsyncProducto(dto));

        Assert.Contains("El stock debe ser mayor a 0", ex.Message);

        _repositorioMock.Verify(r => r.InsertarProductoAsync(It.IsAny<Producto>()), Times.Never);
    }
}
