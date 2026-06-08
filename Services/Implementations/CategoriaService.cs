using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Implementations;

public class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepositorio _categoriaRepositorio;
    private readonly IValidadorCategoria _validador;

    public CategoriaService(ICategoriaRepositorio categoriaRepositorio, IValidadorCategoria validador)
    {
        _categoriaRepositorio = categoriaRepositorio;
        _validador = validador;
    }

    public async Task<IEnumerable<CategoriaDTO>> ObtenerTodosAsyncCategoria()
    {
        var categorias = await _categoriaRepositorio.BuscarCategoriasAsync();
        return categorias.Select(c => new CategoriaDTO { Id = c.Id, NombreCategoria = c.NombreCategoria });
    }

    public async Task<CategoriaDTO?> ObtenerPorIdAsyncCategoria(int id)
    {
        var categoria = await _categoriaRepositorio.BuscarCategoriaPorIdAsync(id);
        if (categoria == null) return null;
        return MapToDTO(categoria);
    }

    public async Task<CategoriaDTO> CrearAsyncCategoria(CategoriaCreateDTO dto)
    {
        var erroresNegocio = await _validador.ValidarDatosCategoria(dto, null);
        if (erroresNegocio.Any()) throw new InvalidOperationException(string.Join(", ", erroresNegocio));

        var categoria = new Categoria { NombreCategoria = dto.NombreCategoria };
        categoria = await _categoriaRepositorio.InsertarCategoriaAsync(categoria);
        return MapToDTO(categoria);
    }

    public async Task<CategoriaDTO?> ActualizarAsyncCategoria(int id, CategoriaCreateDTO dto)
    {
        var categoria = await _categoriaRepositorio.BuscarCategoriaPorIdAsync(id);
        if (categoria == null) return null;

        var erroresNegocio = await _validador.ValidarDatosCategoria(dto, id);
        if (erroresNegocio.Any()) throw new InvalidOperationException(string.Join(", ", erroresNegocio));

        categoria.NombreCategoria = dto.NombreCategoria;
        categoria = await _categoriaRepositorio.ModificarCategoriaAsync(categoria);
        return MapToDTO(categoria);
    }

    public async Task<bool> EliminarAsyncCategoria(int id)
    {
        return await _categoriaRepositorio.EliminarCategoriaFisicoAsync(id);
    }

    public async Task<bool> PuedeEliminarAsync(int id)
    {
        return !await _categoriaRepositorio.VerificarProductosActivosAsync(id);
    }

    private CategoriaDTO MapToDTO(Categoria categoria) => new CategoriaDTO
    {
        Id = categoria.Id, NombreCategoria = categoria.NombreCategoria
    };
}