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
    private readonly IValidadorCategoria _validador;

    public CategoriasController(ICategoriaService categoriaService, IValidadorCategoria validador)
    {
        _categoriaService = categoriaService;
        _validador = validador;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> ObtenerTodasCategorias()
    {
        var categorias = await _categoriaService.ObtenerTodosAsyncCategoria();
        return Ok(categorias);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoriaDTO>> ObtenerCategoriaPorId(int id)
    {
        var categoria = await _categoriaService.ObtenerPorIdAsyncCategoria(id);
        if (categoria == null)
            return NotFound(new { message = "Categoría no encontrada" });
        return Ok(categoria);
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaDTO>> CrearCategoria([FromBody] CategoriaCreateDTO categoria)
    {
        var errores = await _validador.ValidarDatosCategoria(categoria, null);
        if (errores.Count > 0)
            return BadRequest(new { success = false, message = "Error de validación", errors = errores });

        try
        {
            var created = await _categoriaService.CrearAsyncCategoria(categoria);
            return CreatedAtAction(nameof(ObtenerCategoriaPorId), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CategoriaDTO>> ActualizarCategoria(int id, [FromBody] CategoriaCreateDTO categoria)
    {
        var errores = await _validador.ValidarDatosCategoria(categoria, id);
        if (errores.Count > 0)
            return BadRequest(new { success = false, message = "Error de validación", errors = errores });

        var updated = await _categoriaService.ActualizarAsyncCategoria(id, categoria);
        if (updated == null)
            return NotFound(new { message = "Categoría no encontrada" });
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> EliminarCategoria(int id)
    {
        var puedeEliminar = await _categoriaService.PuedeEliminarAsync(id);
        if (!puedeEliminar)
            return BadRequest(new { message = "No se puede eliminar, tiene productos vinculados" });

        var result = await _categoriaService.EliminarAsyncCategoria(id);
        if (!result)
            return NotFound(new { message = "Categoría no encontrada" });
        return Ok(new { message = "Categoría eliminada" });
    }
}