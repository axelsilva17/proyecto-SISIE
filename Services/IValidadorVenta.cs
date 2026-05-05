using System.Collections.Generic;
using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services;

// Validador para Venta
public interface IValidadorVenta
{
    Task<List<string>> ValidarAsync(VentaCreateDTO dto);
    Task<List<string>> ValidarActualizacionAsync(VentaUpdateDTO dto);
}

public class ValidadorVenta : IValidadorVenta
{
    public Task<List<string>> ValidarAsync(VentaCreateDTO dto)
    {
        var errores = new List<string>();
        
        if (dto.Detalles == null || dto.Detalles.Count == 0)
            errores.Add("Debe incluir al menos un producto");
        
        if (dto.Detalles != null)
        {
            foreach (var detalle in dto.Detalles)
            {
                if (detalle.IdProducto <= 0)
                    errores.Add("Producto inválido");
                if (detalle.Cantidad <= 0)
                    errores.Add("La cantidad debe ser mayor a 0");
            }
        }
        
        if (string.IsNullOrWhiteSpace(dto.MetodoPago))
            errores.Add("El método de pago es obligatorio");
        
        return Task.FromResult(errores);
    }

    public Task<List<string>> ValidarActualizacionAsync(VentaUpdateDTO dto)
    {
        var errores = new List<string>();
        
        if (string.IsNullOrWhiteSpace(dto.Estado))
            errores.Add("El estado es obligatorio");
        
        var estadosValidos = new[] { "Pendiente", "Pagada", "Enviada", "Entregada", "Cancelada" };
        if (!string.IsNullOrWhiteSpace(dto.Estado) && !estadosValidos.Contains(dto.Estado))
            errores.Add($"Estado inválido. Estados válidos: {string.Join(", ", estadosValidos)}");
        
        return Task.FromResult(errores);
    }
}