using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Services;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;
    private readonly IValidadorProducto _validador;

    public ProductosController(IProductoService productoService, IValidadorProducto validador)
    {
        _productoService = productoService;
        _validador = validador;
    }

    [HttpGet]
    public async Task<ActionResult<ProductoPagedResult>> ObtenerTodosProductos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? idCategoria = null,
        [FromQuery] bool? activo = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var (items, total) = await _productoService.ObtenerTodosAsyncProducto(page, pageSize, idCategoria, activo);
        return Ok(new ProductoPagedResult { Items = items, Total = total, Page = page, PageSize = pageSize });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductoDTO>> ObtenerProductoPorId(int id)
    {
        var producto = await _productoService.ObtenerPorIdAsyncProducto(id);
        if (producto == null)
            return NotFound(new { message = "Producto no encontrado" });
        return Ok(producto);
    }

    [HttpPost]
    public async Task<ActionResult<ProductoDTO>> CrearProducto([FromBody] ProductoCreateDTO producto)
    {
        // Validar campos obligatorios
        var errores = await _validador.ValidarDatosProductoCreate(producto);
        if (errores.Count > 0)
            return BadRequest(new { success = false, message = "Error de validación", errors = errores });

        // Validar reglas de negocio (duplicado, categoría existe)
        var erroresNegocio = await _validador.ValidaProducto(producto);
        if (erroresNegocio.Count > 0)
            return BadRequest(new { success = false, message = erroresNegocio[0], errors = erroresNegocio });

        var created = await _productoService.CrearAsyncProducto(producto);
        return CreatedAtAction(nameof(ObtenerProductoPorId), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductoDTO>> ActualizarProducto(int id, [FromBody] ProductoUpdateDTO producto)
    {
        var errores = await _validador.ValidarDatosProductoUpdate(producto);
        if (errores.Count > 0)
            return BadRequest(new { success = false, message = "Error de validación", errors = errores });

        // Validar reglas de negocio (duplicado, categoría existe)
        var erroresNegocio = await _validador.ValidaProductoUpdate(producto, id);
        if (erroresNegocio.Count > 0)
            return BadRequest(new { success = false, message = erroresNegocio[0], errors = erroresNegocio });

        var updated = await _productoService.ActualizarAsyncProducto(id, producto);
        if (updated == null)
            return NotFound(new { message = "Producto no encontrado" });
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> EliminarProducto(int id)
    {
        var result = await _productoService.EliminarAsyncProducto(id);
        if (!result)
            return NotFound(new { message = "Producto no encontrado" });
        return Ok(new { message = "Producto eliminado" });
    }

    [HttpPatch("{id}/toggle")]
    public async Task<ActionResult> ToggleActivoProducto(int id)
    {
        var producto = await _productoService.ObtenerPorIdAsyncProducto(id);
        if (producto == null)
            return NotFound(new { message = "Producto no encontrado" });

        var updated = await _productoService.ToggleActivoAsyncProducto(id);
        if (updated == null)
            return NotFound(new { message = "Producto no encontrado" });

        return Ok(new { activo = updated.Activo, message = updated.Activo ? "Producto activado" : "Producto desactivado" });
    }
}