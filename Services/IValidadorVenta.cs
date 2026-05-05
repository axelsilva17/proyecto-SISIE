using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services;

public interface IValidadorVenta
{
    Task<List<string>> ValidarDatosVenta(VentaCreateDTO dto);
    Task<List<string>> ValidarDatosVentaCreate(VentaCreateDTO dto, int idUsuario);
    Task<List<string>> ValidarDatosVentaUpdate(VentaUpdateDTO dto);
}

public class ValidadorVenta : IValidadorVenta
{
    private readonly ApplicationDbContext _context;
    
    public ValidadorVenta(ApplicationDbContext context) => _context = context;

    public Task<List<string>> ValidarDatosVenta(VentaCreateDTO dto)
    {
        var errores = new List<string>();
        if (dto.Detalles == null || dto.Detalles.Count == 0) errores.Add("Debe incluir al menos un producto");
        
        dto.Detalles?.ForEach(d => {
            if (d.IdProducto <= 0) errores.Add("Producto inválido");
            if (d.Cantidad <= 0) errores.Add("La cantidad debe ser mayor a 0");
        });
        
        if (string.IsNullOrWhiteSpace(dto.MetodoPago)) errores.Add("El método de pago es obligatorio");
        
        return Task.FromResult(errores);
    }
    
    public Task<List<string>> ValidarDatosVentaUpdate(VentaUpdateDTO dto)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Estado)) errores.Add("El estado es obligatorio");
        
        var estadosValidos = new[] { "Pendiente", "Pagada", "Enviada", "Entregada", "Cancelada" };
        if (!string.IsNullOrWhiteSpace(dto.Estado) && !estadosValidos.Contains(dto.Estado))
            errores.Add($"Estado inválido. Estados válidos: {string.Join(", ", estadosValidos)}");
        
        return Task.FromResult(errores);
    }

    public async Task<List<string>> ValidarDatosVentaCreate(VentaCreateDTO dto, int idUsuario)
    {
        var errores = new List<string>();
        
        var usuario = await _context.Usuarios.FindAsync(idUsuario);
        if (usuario == null) errores.Add("El usuario no existe");

        if (dto.Detalles == null || !dto.Detalles.Any()) errores.Add("Debe incluir al menos un producto");

        if (dto.EsEnvio)
        {
            if (!dto.IdCiudad.HasValue || dto.IdCiudad.Value <= 0)
                errores.Add("Debe seleccionar una ciudad para envío a domicilio");
            if (!dto.IdDireccion.HasValue && string.IsNullOrWhiteSpace(dto.DireccionEnvio))
                errores.Add("Debe indicar la dirección para envío a domicilio");
        }

        if (dto.IdDireccion.HasValue)
        {
            var direccionValida = await _context.Direcciones.AnyAsync(d => d.Id == dto.IdDireccion.Value);
            if (!direccionValida) errores.Add("La dirección no es válida");
        }

        foreach (var detalle in dto.Detalles ?? Enumerable.Empty<VentaDetalleDTO>())
        {
            var producto = await _context.Productos.FindAsync(detalle.IdProducto);
            if (producto == null) errores.Add($"El producto con ID {detalle.IdProducto} no existe");
            else if (!producto.Activo) errores.Add($"El producto '{producto.NombreProducto}' está inactivo");
            else if (producto.Stock < detalle.Cantidad)
                errores.Add($"Stock insuficiente para '{producto.NombreProducto}'. Disponible: {producto.Stock}");
        }
        
        return errores;
    }
}