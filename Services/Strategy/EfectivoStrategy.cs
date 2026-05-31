namespace proyecto_SISIE.Services.Strategy;

/// <summary>
/// Estrategia para pago en efectivo. Sin recargos.
/// </summary>
public class EfectivoStrategy : IMetodoPagoStrategy
{
    public string MetodoPago => "Efectivo";

    public decimal CalcularTotal(decimal subtotal, int descuento)
    {
        var descuentoDecimal = (decimal)descuento / 100m;
        return Math.Round(subtotal * (1m - descuentoDecimal), 2);
    }
}
