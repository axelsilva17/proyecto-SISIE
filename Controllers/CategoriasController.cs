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

    // Lista todas las categorías
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> ObtenerTodasCategorias()
    {
        // Obtiene todas las categorías del service
        var categorias = await _categoriaService.ObtenerTodosAsyncCategoria();
        return Ok(categorias);
    }

    // Obtiene una categoría por su ID
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoriaDTO>> ObtenerCategoriaPorId(int id)
    {
        // Busca la categoría
        var categoria = await _categoriaService.ObtenerPorIdAsyncCategoria(id);
        
        // Si no existe, retorna 404
        if (categoria == null)
            return NotFound(new { message = "Categoría no encontrada" });

        return Ok(categoria);
    }

    // Crea una nueva categoría
    [HttpPost]
    public async Task<ActionResult<CategoriaDTO>> CrearCategoria([FromBody] CategoriaCreateDTO categoria)
    {
        // Valida los datos
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
            // Intenta crear la categoría
            var created = await _categoriaService.CrearAsyncCategoria(categoria);
            
            // Retorna 201 con la ubicación del recurso
            return CreatedAtAction(nameof(ObtenerCategoriaPorId), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            // Captura error de nombre duplicado
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // Actualiza una categoría existente
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoriaDTO>> ActualizarCategoria(int id, [FromBody] CategoriaCreateDTO categoria)
    {
        // Valida los datos
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

        // Actualiza la categoría
        var updated = await _categoriaService.ActualizarAsyncCategoria(id, categoria);
        
        // Si no existe, retorna 404
        if (updated == null)
            return NotFound(new { message = "Categoría no encontrada" });

        return Ok(updated);
    }

    // Elimina una categoría
    [HttpDelete("{id}")]
    public async Task<ActionResult> EliminarCategoria(int id)
    {
        // Verifica si tiene productos activos
        var puedeEliminar = await _categoriaService.PuedeEliminarAsync(id);
        
        // Si tiene productos, no permite eliminar
        if (!puedeEliminar)
            return BadRequest(new { message = "No se puede eliminar, tiene productos vinculados" });

        // Intenta eliminar
        var result = await _categoriaService.EliminarAsyncCategoria(id);
        
        // Si no existía, retorna 404
        if (!result)
            return NotFound(new { message = "Categoría no encontrada" });

        return Ok(new { message = "Categoría eliminada" });
    }
}