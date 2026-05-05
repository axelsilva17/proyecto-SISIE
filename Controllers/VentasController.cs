using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Services;
using proyecto_SISIE.Services.Interfaces;
using System.Security.Claims;

namespace proyecto_SISIE.Controllers;

// Controlador para gestionar ventas
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VentasController : ControllerBase
{
    private readonly IVentaService _ventaService;
    private readonly IClienteService _clienteService;
    private readonly IValidadorVenta _validador;

    public VentasController(IVentaService ventaService, IClienteService clienteService, IValidadorVenta validador)
    {
        _ventaService = ventaService;
        _clienteService = clienteService;
        _validador = validador;
    }

    // Obtiene el ID del usuario autenticado desde el token JWT
    private int ObtenerIdUsuario()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null)
            throw new UnauthorizedAccessException("No se encontró el ID del usuario en el token");

        // El claim puede ser string o int - intentá parsear
        if (int.TryParse(claim.Value, out int idInt))
            return idInt;

        // Si es string (GUID de AspNetUsers), usar un ID temporal
        // Por ahora retornamos 1 como default - después mejoramos esto
        return 1;
    }

    // Registra una nueva venta con sus productos
    [HttpPost("registrar")]
    public async Task<ActionResult<VentaDTO>> RegistrarVenta([FromBody] VentaCreateDTO ventaDto)
    {
        // Valida usando el validador desacoplado
        var errores = await _validador.ValidarAsync(ventaDto);
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
            // Obtiene el ID del usuario logueado
            var idUsuario = ObtenerIdUsuario();

            // Registra la venta
            var venta = await _ventaService.RegistrarVentaAsync(idUsuario, ventaDto);
            
            // Retorna 201 con la ubicación del recurso
            return CreatedAtAction(nameof(ObtenerVentaPorId), new { id = venta.Id }, venta);
        }
        catch (InvalidOperationException ex)
        {
            // Captura errores de negocio (stock insuficiente, datos inválidos, etc.)
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            // Captura cualquier otro error
            return StatusCode(500, new { success = false, message = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    // Obtiene una venta específica por su ID
    [HttpGet("{id}")]
    public async Task<ActionResult<VentaDTO>> ObtenerVentaPorId(int id)
    {
        // Busca la venta
        var venta = await _ventaService.ObtenerVentaPorIdAsync(id);
        
        // Si no existe, retorna 404
        if (venta == null)
            return NotFound(new { message = "Venta no encontrada" });

        return Ok(venta);
    }

    // Obtiene el historial de ventas con paginación y filtros
    [HttpGet("historial")]
    public async Task<ActionResult<VentaPagedResult>> ObtenerHistorialVentas(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? idUsuario = null,
        [FromQuery] string? estado = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null)
    {
        // Normaliza los parámetros de paginación
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        try
        {
            // Obtiene el historial de ventas
            var (items, total) = await _ventaService.ObtenerHistorialVentasAsync(
                page, pageSize, idUsuario, estado, fechaDesde, fechaHasta);

            // Retorna con metadatos de paginación
            return Ok(new VentaPagedResult
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // Obtiene las ventas del usuario logueado
    [HttpGet("mis-ventas")]
    public async Task<ActionResult<VentaPagedResult>> ObtenerMisVentas(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        // Normaliza los parámetros de paginación
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        try
        {
            var idUsuario = ObtenerIdUsuario();
            var resultado = await _ventaService.ObtenerVentasPorUsuarioAsync(idUsuario, page, pageSize);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // Actualiza el estado de una venta (Pendiente -> Entregada, Cancelada, etc.)
    [HttpPut("{id}/estado")]
    public async Task<ActionResult<VentaDTO>> ActualizarEstadoVenta(int id, [FromBody] VentaUpdateDTO updateDto)
    {
        // Valida usando el validador desacoplado
        var errores = await _validador.ValidarActualizacionAsync(updateDto);
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
            // Actualiza el estado de la venta
            var venta = await _ventaService.ActualizarEstadoVentaAsync(id, updateDto);
            
            // Si no existe, retorna 404
            if (venta == null)
                return NotFound(new { message = "Venta no encontrada" });

            return Ok(venta);
        }
        catch (InvalidOperationException ex)
        {
            // Captura errores de negocio (venta ya cancelada, etc.)
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // Cancela una venta y reposiciona el stock
    [HttpPut("{id}/cancelar")]
    public async Task<ActionResult<VentaDTO>> CancelarVenta(int id)
    {
        try
        {
            // Cancela la venta
            var venta = await _ventaService.CancelarVentaAsync(id);
            
            // Si no existe, retorna 404
            if (venta == null)
                return NotFound(new { message = "Venta no encontrada" });

            return Ok(venta);
        }
        catch (InvalidOperationException ex)
        {
            // Captura errores de negocio (venta ya cancelada,etc.)
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // Verifica el stock de un producto antes de agregarlo al carrito
    [HttpGet("verificar-stock/{idProducto}/{cantidad}")]
    public async Task<ActionResult<StockVerificacionDTO>> VerificarStockProducto(int idProducto, int cantidad)
    {
        // Valida que la cantidad sea positiva
        if (cantidad <= 0)
            return BadRequest(new { success = false, message = "La cantidad debe ser mayor a 0" });

        try
        {
            var verificacion = await _ventaService.VerificarStockProductoAsync(idProducto, cantidad);
            return Ok(verificacion);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // Verifica el stock de varios productos (para el carrito)
    [HttpPost("verificar-carrito")]
    public async Task<ActionResult<CarritoVerificacionDTO>> VerificarStockCarrito([FromBody] List<VentaDetalleDTO> detalles)
    {
        // Valida que haya productos
        if (detalles == null || !detalles.Any())
            return BadRequest(new { success = false, message = "Debe incluir al menos un producto" });

        try
        {
            var resultado = await _ventaService.VerificarStockCarritoAsync(detalles);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // Obtiene estadísticas de ventas para el dashboard
    [HttpGet("estadisticas")]
    public async Task<ActionResult<object>> ObtenerEstadisticas(
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null)
    {
        try
        {
            var estadisticas = await _ventaService.ObtenerEstadisticasVentasAsync(fechaDesde, fechaHasta);
            return Ok(estadisticas);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

// Busca un cliente por DNI (para autocompletar en el formulario de ventas)
    [HttpGet("buscar-cliente/{dni}")]
    public async Task<ActionResult<ClienteDTO>> BuscarClientePorDni(string dni)
    {
        try
        {
            var cliente = await _clienteService.BuscarPorDniAsync(dni);
            
            if (cliente == null)
                return NotFound(new { message = "Cliente no encontrado" });

            return Ok(cliente);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
