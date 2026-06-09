using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Implementations;

public class ValidadorCategoria : IValidadorCategoria
{
    private readonly ICategoriaRepositorio _categoriaRepositorio;

    public ValidadorCategoria(ICategoriaRepositorio categoriaRepositorio) => _categoriaRepositorio = categoriaRepositorio;

    public async Task<List<string>> ValidarDatosCategoria(CategoriaCreateDTO dto, int? idCategoria)
    {
        var errores = new List<string>();
        errores.AddRange(ValidarNombreFormato(dto.NombreCategoria));
        errores.AddRange(await ValidarNombreUnico(dto.NombreCategoria, idCategoria));
        return errores;
    }

    public List<string> ValidarNombreFormato(string nombre)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(nombre)) errores.Add("El nombre es obligatorio");
        else if (nombre.Length > 50) errores.Add("El nombre no puede superar los 50 caracteres");
        return errores;
    }

    private async Task<List<string>> ValidarNombreUnico(string nombre, int? idCategoria)
    {
        var errores = new List<string>();
        var existeNombre = await _categoriaRepositorio.VerificarNombreCategoriaExisteAsync(nombre, idCategoria);
        if (existeNombre) errores.Add("Ya existe una categoría con ese nombre");
        return errores;
    }

    public async Task<List<string>> ValidarCategoriaExiste(int idCategoria)
    {
        var existe = await _categoriaRepositorio.VerificarCategoriaExisteAsync(idCategoria);
        return existe ? [] : ["La categoría no existe"];
    }
}
