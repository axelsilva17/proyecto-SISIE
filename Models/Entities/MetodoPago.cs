using System.ComponentModel.DataAnnotations;

namespace proyecto_SISIE.Models.Entities;

/// <summary>
/// Tabla: MetodoPago
/// Almacena los métodos de pago disponibles (Efectivo, Tarjeta, Transferencia, etc.)
/// </summary>
public class MetodoPago
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Porcentaje de recargo (0 = sin recargo, 3 = 3%, etc.)
    /// </summary>
    public decimal RecargoPorcentaje { get; set; }

    /// <summary>
    /// Si el método de pago está activo para nuevas ventas
    /// </summary>
    public bool Activo { get; set; } = true;
}
