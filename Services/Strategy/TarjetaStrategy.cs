namespace proyecto_SISIE.Services.Strategy;

/// <summary>
/// Estrategia para pago con tarjeta de crédito/débito.
/// Aplica un recargo del 3% sobre el total con descuento.
/// </summary>
public class TarjetaStrategy : IMetodoPagoStrategy
{
    private const decimal Recargo = 0.03m;

    public string MetodoPago => "Tarjeta";

    public decimal CalcularTotal(decimal subtotal, int descuento)
    {
        var descuentoDecimal = (decimal)descuento / 100m;
        var totalConDescuento = subtotal * (1m - descuentoDecimal);
        var totalConRecargo = totalConDescuento * (1m + Recargo);
        return Math.Round(totalConRecargo, 2);
    }
}
