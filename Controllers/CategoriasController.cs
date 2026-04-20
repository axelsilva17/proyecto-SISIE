using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetAllCategorias()
    {
        var categorias = await _categoriaService.GetAllAsync();
        return Ok(categorias);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoriaDTO>> GetCategoriaById(int id)
    {
        var categoria = await _categoriaService.GetByIdAsync(id);
        if (categoria == null)
            return NotFound(new { message = "Categoría no encontrada" });

        return Ok(categoria);
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaDTO>> CreateCategoria([FromBody] CategoriaCreateDTO categoria)
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
            var created = await _categoriaService.CreateAsync(categoria);
            return CreatedAtAction(nameof(GetCategoriaById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CategoriaDTO>> UpdateCategoria(int id, [FromBody] CategoriaCreateDTO categoria)
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

        var updated = await _categoriaService.UpdateAsync(id, categoria);
        if (updated == null)
            return NotFound(new { message = "Categoría no encontrada" });

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCategoria(int id)
    {
        // Verificar si tiene productos activos
        var canDelete = await _categoriaService.CanDeleteAsync(id);
        if (!canDelete)
            return BadRequest(new { message = "No se puede eliminar, tiene productos vinculados" });

        var result = await _categoriaService.DeleteAsync(id);
        if (!result)
            return NotFound(new { message = "Categoría no encontrada" });

        return Ok(new { message = "Categoría eliminada" });
    }
}