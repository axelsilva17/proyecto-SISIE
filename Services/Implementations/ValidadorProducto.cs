using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Implementations;

public class ValidadorProducto : IValidadorProducto
{
    private readonly IProductoRepositorio _productoRepositorio;
    private readonly IValidadorCategoria _validadorCategoria;

    public ValidadorProducto(IProductoRepositorio productoRepositorio, IValidadorCategoria validadorCategoria)
    {
        _productoRepositorio = productoRepositorio;
        _validadorCategoria = validadorCategoria;
    }

    public Task<List<string>> ValidarDatosProductoCreate(ProductoCreateDTO dto)
    {
        var errores = new List<string>();
        errores.AddRange(ValidarNombre(dto.NombreProducto));
        errores.AddRange(ValidarPrecio(dto.PrecioUnitario));
        errores.AddRange(ValidarStockFormato(dto.Stock));
        if (string.IsNullOrWhiteSpace(dto.Descripcion)) errores.Add("La descripción es obligatoria");
        return Task.FromResult(errores);
    }

    public Task<List<string>> ValidarDatosProductoUpdate(ProductoUpdateDTO dto)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.NombreProducto)) errores.Add("El nombre es obligatorio");
        if (dto.PrecioUnitario <= 0) errores.Add("El precio debe ser mayor a 0");
        if (dto.Stock < 0) errores.Add("El stock no puede ser negativo");
        return Task.FromResult(errores);
    }

    public async Task<List<string>> ValidaProducto(ProductoCreateDTO dto, int? idProducto = null)
    {
        var errores = new List<string>();
        errores.AddRange(await ValidarNombreUnico(dto.NombreProducto, idProducto));
        errores.AddRange(await _validadorCategoria.ValidarCategoriaExiste(dto.IdCategoria));
        return errores;
    }

    public async Task<List<string>> ValidaProductoUpdate(ProductoUpdateDTO dto, int idProducto)
    {
        var errores = new List<string>();

        if (!await _productoRepositorio.VerificarProductoExisteAsync(idProducto))
        {
            errores.Add("El producto no existe");
            return errores;
        }

        errores.AddRange(await ValidarNombreUnico(dto.NombreProducto, idProducto));
        errores.AddRange(await _validadorCategoria.ValidarCategoriaExiste(dto.IdCategoria));

        return errores;
    }

    private List<string> ValidarNombre(string nombre)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(nombre)) errores.Add("El nombre es obligatorio");
        return errores;
    }

    private List<string> ValidarPrecio(decimal precio)
    {
        var errores = new List<string>();
        if (precio <= 0) errores.Add("El precio debe ser mayor a 0");
        return errores;
    }

    private List<string> ValidarStockFormato(int stock)
    {
        var errores = new List<string>();
        if (stock <= 0) errores.Add("El stock debe ser mayor a 0");
        return errores;
    }

    private async Task<List<string>> ValidarNombreUnico(string nombre, int? idProducto)
    {
        var errores = new List<string>();
        var existeNombre = await _productoRepositorio.VerificarNombreProductoExisteAsync(nombre, idProducto);
        if (existeNombre) errores.Add("Ya existe un producto con ese nombre");
        return errores;
    }
}
