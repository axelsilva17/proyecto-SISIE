using Moq;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Implementations;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Tests;

public class CategoriaServiceTests
{
    // Configuración de los mocks y la instancia del servicio
    private readonly Mock<ICategoriaRepositorio> _repositorioMock;
    private readonly Mock<IValidadorCategoria> _validadorMock;
    private readonly CategoriaService _service;

    public CategoriaServiceTests()
    {
        _repositorioMock = new Mock<ICategoriaRepositorio>();
        _validadorMock = new Mock<IValidadorCategoria>();
        // Creación de la instancia del servicio con los mocks
        _service = new CategoriaService(_repositorioMock.Object, _validadorMock.Object);
    }

    [Fact]
    public async Task CrearCategoria_DatosValidos_RetornaCategoriaDTO()
    {
        var dto = new CategoriaCreateDTO { NombreCategoria = "Limpieza" };
        var categoria = new Categoria { Id = 6, NombreCategoria = "Limpieza" };

        _validadorMock
            .Setup(v => v.ValidarDatosCategoria(dto, null))
            .ReturnsAsync(new List<string>());

        _repositorioMock
            .Setup(r => r.InsertarCategoriaAsync(It.IsAny<Categoria>()))
            .ReturnsAsync(categoria);

        var result = await _service.CrearAsyncCategoria(dto);

        Assert.NotNull(result);
        Assert.Equal(6, result.Id);
        Assert.Equal("Limpieza", result.NombreCategoria);

        _repositorioMock.Verify(r => r.InsertarCategoriaAsync(It.IsAny<Categoria>()), Times.Once);
    }

    [Fact]
    public async Task CrearCategoria_NombreDuplicado_LanzaExcepcion()
    {
        // Simulación de una validación que detecta un nombre de categoría duplicado
        var dto = new CategoriaCreateDTO { NombreCategoria = "Herramientas Manuales" };
        var errores = new List<string> { "Ya existe una categoría con ese nombre" };

        _validadorMock
            .Setup(v => v.ValidarDatosCategoria(dto, null))
            .ReturnsAsync(errores);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CrearAsyncCategoria(dto));

        Assert.Contains("Ya existe una categoría con ese nombre", ex.Message);

        _repositorioMock.Verify(r => r.InsertarCategoriaAsync(It.IsAny<Categoria>()), Times.Never);
    }

    [Fact]
    public async Task CrearCategoria_NombreVacio_LanzaExcepcion()
    {
        var dto = new CategoriaCreateDTO { NombreCategoria = "" };
        var errores = new List<string> { "El nombre es obligatorio" };

        _validadorMock
            .Setup(v => v.ValidarDatosCategoria(dto, null))
            .ReturnsAsync(errores);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CrearAsyncCategoria(dto));

        Assert.Contains("El nombre es obligatorio", ex.Message);

        _repositorioMock.Verify(r => r.InsertarCategoriaAsync(It.IsAny<Categoria>()), Times.Never);
    }

}
