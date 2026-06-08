using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Helpers;
using proyecto_SISIE.Services.Interfaces;
using System.Security.Claims;

namespace proyecto_SISIE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VentasController : ControllerBase
{
    private readonly IVentaService _ventaService;
    private readonly IProductoService _productoService;
    private readonly IClienteService _clienteService;
    private readonly IValidadorVenta _validador;
    private readonly IAuthService _authService;

    public VentasController(IVentaService ventaService, IProductoService productoService, IClienteService clienteService, IValidadorVenta validador, IAuthService authService)
    {
        _ventaService = ventaService;
        _productoService = productoService;
        _clienteService = clienteService;
        _validador = validador;
        _authService = authService;
    }

    private async Task<int> ObtenerIdUsuarioAsync()
    {
        var userName = User.FindFirst(ClaimTypes.Name)?.Value
            ?? throw new UnauthorizedAccessException("No se pudo identificar al usuario en la sesión");

        var idUsuario = await _authService.ObtenerIdUsuarioPorNombreAsync(userName);
        return idUsuario ?? throw new UnauthorizedAccessException("El usuario no tiene permisos para realizar ventas");
    }

    [HttpPost("registrar")]
    public async Task<ActionResult<VentaDTO>> RegistrarVenta([FromBody] VentaCreateDTO ventaDto)
    {
        int idUsuario;
        try
        {
            idUsuario = await ObtenerIdUsuarioAsync();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, message = ex.Message });
        }

        var errores = await _validador.ValidarDatosVenta(ventaDto, idUsuario);
        if (errores.Count > 0)
            return BadRequest(new { success = false, message = "Error de validación", errors = errores });

        try
        {
            var venta = await _ventaService.RegistrarVentaAsync(idUsuario, ventaDto);
            return CreatedAtAction(nameof(ObtenerVentaPorId), new { id = venta.Id }, venta);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VentaDTO>> ObtenerVentaPorId(int id)
    {
        var venta = await _ventaService.ObtenerVentaPorIdAsync(id);
        if (venta == null)
            return NotFound(new { message = "Venta no encontrada" });
        return Ok(venta);
    }

    [HttpGet("historial")]
    public async Task<ActionResult<PagedResult<VentaHistorialDTO>>> ObtenerHistorialVentas(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? idUsuario = null,
        [FromQuery] string? estado = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null)
    {
        (page, pageSize) = PageHelper.Clamp(page, pageSize);

        try
        {
            var (items, total) = await _ventaService.ObtenerHistorialVentasAsync(page, pageSize, idUsuario, estado, fechaDesde, fechaHasta);
            return Ok(new PagedResult<VentaHistorialDTO> { Items = items, Total = total, Page = page, PageSize = pageSize });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("mis-ventas")]
    public async Task<ActionResult<PagedResult<VentaHistorialDTO>>> ObtenerMisVentas([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        (page, pageSize) = PageHelper.Clamp(page, pageSize);

        try
        {
            var idUsuario = await ObtenerIdUsuarioAsync();
            var resultado = await _ventaService.ObtenerVentasPorUsuarioAsync(idUsuario, page, pageSize);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("{id}/estado")]
    public async Task<ActionResult<VentaDTO>> ActualizarEstadoVenta(int id, [FromBody] VentaUpdateDTO updateDto)
    {
        var errores = await _validador.ValidarDatosVentaUpdate(updateDto);
        if (errores.Count > 0)
            return BadRequest(new { success = false, message = "Error de validación", errors = errores });

        try
        {
            var venta = await _ventaService.ActualizarEstadoVentaAsync(id, updateDto);
            if (venta == null)
                return NotFound(new { message = "Venta no encontrada" });
            return Ok(venta);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("{id}/cancelar")]
    public async Task<ActionResult<VentaDTO>> CancelarVenta(int id)
    {
        try
        {
            var venta = await _ventaService.CancelarVentaAsync(id);
            if (venta == null)
                return NotFound(new { message = "Venta no encontrada" });
            return Ok(venta);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("verificar-stock/{idProducto}/{cantidad}")]
    public async Task<ActionResult<StockVerificacionDTO>> VerificarStockProducto(int idProducto, int cantidad)
    {
        if (cantidad <= 0)
            return BadRequest(new { success = false, message = "La cantidad debe ser mayor a 0" });

        try
        {
            var verificacion = await _productoService.VerificarStockProductoAsync(idProducto, cantidad);
            return Ok(verificacion);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("verificar-carrito")]
    public async Task<ActionResult<CarritoVerificacionDTO>> VerificarStockCarrito([FromBody] List<VentaDetalleDTO> detalles)
    {
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

    [HttpGet("estadisticas")]
    public async Task<ActionResult<object>> ObtenerEstadisticas([FromQuery] DateTime? fechaDesde = null, [FromQuery] DateTime? fechaHasta = null)
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