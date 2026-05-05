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

    // Lista productos con paginación
    [HttpGet]
    public async Task<ActionResult<ProductoPagedResult>> ObtenerTodosProductos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? idCategoria = null,
        [FromQuery] bool? activo = null)
    {
        // Normaliza los parámetros de paginación
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        // Llama al service para obtener productos
        var (items, total) = await _productoService.ObtenerTodosAsyncProducto(page, pageSize, idCategoria, activo);

        // Retorna con metadatos de paginación
        return Ok(new ProductoPagedResult
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        });
    }

    // Obtiene un producto por su ID
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductoDTO>> ObtenerProductoPorId(int id)
    {
        // Busca el producto
        var producto = await _productoService.ObtenerPorIdAsyncProducto(id);
        
        // Si no existe, retorna 404
        if (producto == null)
            return NotFound(new { message = "Producto no encontrado" });

        return Ok(producto);
    }

    // Crea un nuevo producto
    [HttpPost]
    public async Task<ActionResult<ProductoDTO>> CrearProducto([FromBody] ProductoCreateDTO producto)
    {
        // Valida usando el validador desacoplado
        var errores = await _validador.ValidarAsync(producto);
        if (errores.Count > 0)
        {
            return BadRequest(new { 
                success = false, 
                message = "Error de validación",
                errors = errores 
            });
        }

        try
        {
            // Intenta crear el producto
            var created = await _productoService.CrearAsyncProducto(producto);
            
            // Retorna 201 con la ubicación del recurso
            return CreatedAtAction(nameof(ObtenerProductoPorId), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            // Captura errores de negocio (duplicado, etc)
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // Actualiza un producto existente
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductoDTO>> ActualizarProducto(int id, [FromBody] ProductoUpdateDTO producto)
    {
        // Valida usando el validador desacoplado
        var errores = await _validador.ValidarAsync(producto);
        if (errores.Count > 0)
        {
            return BadRequest(new { 
                success = false, 
                message = "Error de validación",
                errors = errores 
            });
        }

        // Actualiza el producto
        var updated = await _productoService.ActualizarAsyncProducto(id, producto);
        
        // Si no existe, retorna 404
        if (updated == null)
            return NotFound(new { message = "Producto no encontrado" });

        return Ok(updated);
    }

    // Elimina un producto (soft delete)
    [HttpDelete("{id}")]
    public async Task<ActionResult> EliminarProducto(int id)
    {
        // Intenta eliminar (marca como inactivo)
        var result = await _productoService.EliminarAsyncProducto(id);
        
        // Si no existía, retorna 404
        if (!result)
            return NotFound(new { message = "Producto no encontrado" });

        return Ok(new { message = "Producto eliminado" });
    }

    // Activa o desactiva un producto
    [HttpPatch("{id}/toggle")]
    public async Task<ActionResult> ToggleActivoProducto(int id)
    {
        // Verifica que el producto exista
        var producto = await _productoService.ObtenerPorIdAsyncProducto(id);
        if (producto == null)
            return NotFound(new { message = "Producto no encontrado" });

        // Toggle el estado
        var updated = await _productoService.ToggleActivoAsyncProducto(id);
        if (updated == null)
            return NotFound(new { message = "Producto no encontrado" });

        // Retorna el nuevo estado
        return Ok(new { 
            activo = updated.Activo, 
            message = updated.Activo ? "Producto activado" : "Producto desactivado" 
        });
    }
}