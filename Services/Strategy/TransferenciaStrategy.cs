namespace proyecto_SISIE.Services.Strategy;


/// Estrategia para pago por transferencia bancaria.
/// Aplica un recargo del 1.5% sobre el total con descuento.

public class TransferenciaStrategy : IMetodoPagoStrategy
{
    private const decimal Recargo = 0.015m;

    public string MetodoPago => "Transferencia";

    public decimal CalcularTotal(decimal subtotal, int descuento)
    {
        var descuentoDecimal = (decimal)descuento / 100m;
        var totalConDescuento = subtotal * (1m - descuentoDecimal);
        var totalConRecargo = totalConDescuento * (1m + Recargo);
        return Math.Round(totalConRecargo, 2);
    }
}
