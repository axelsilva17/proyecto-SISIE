using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;

    public ProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpGet]
    public async Task<ActionResult<ProductoPagedResult>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? idCategoria = null,
        [FromQuery] bool? activo = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var (items, total) = await _productoService.GetAllAsync(page, pageSize, idCategoria, activo);

        return Ok(new ProductoPagedResult
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductoDTO>> GetById(int id)
    {
        var producto = await _productoService.GetByIdAsync(id);
        if (producto == null)
            return NotFound(new { message = "Producto no encontrado" });

        return Ok(producto);
    }

    [HttpPost]
    public async Task<ActionResult<ProductoDTO>> Create([FromBody] ProductoCreateDTO producto)
    {
        // Validar con DataAnnotations
        if (!ModelState.IsValid)
        {
            var errores = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => x.Value?.Errors.First().ErrorMessage)
                .ToList();
            
            return BadRequest(new { 
                success = false, 
                message = "Error de validación",
                errors = errores 
            });
        }

        try
        {
            var created = await _productoService.CreateAsync(producto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductoDTO>> Update(int id, [FromBody] ProductoUpdateDTO producto)
    {
        // Validar con DataAnnotations
        if (!ModelState.IsValid)
        {
            var errores = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => x.Value?.Errors.First().ErrorMessage)
                .ToList();
            
            return BadRequest(new { 
                success = false, 
                message = "Error de validación",
                errors = errores 
            });
        }

        var updated = await _productoService.UpdateAsync(id, producto);
        if (updated == null)
            return NotFound(new { message = "Producto no encontrado" });

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _productoService.DeleteAsync(id);
        if (!result)
            return NotFound(new { message = "Producto no encontrado" });

        return Ok(new { message = "Producto eliminado" });
    }

    [HttpPatch("{id}/toggle")]
    public async Task<ActionResult> ToggleActivo(int id)
    {
        var producto = await _productoService.GetByIdAsync(id);
        if (producto == null)
            return NotFound(new { message = "Producto no encontrado" });

        var updated = await _productoService.ToggleActivoAsync(id);
        if (updated == null)
            return NotFound(new { message = "Producto no encontrado" });

        return Ok(new { activo = updated.Activo, message = updated.Activo ? "Producto activado" : "Producto desactivado" });
    }
}