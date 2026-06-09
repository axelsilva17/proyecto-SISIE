namespace proyecto_SISIE.Services.Strategy;

public class TransferenciaStrategy : IMetodoPagoStrategy
{
    private const decimal Recargo = 0.015m;

    public int IdMetodoPago => 3;
    public string NombreMetodoPago => "Transferencia";

    public decimal CalcularTotal(decimal subtotal, int descuento)
    {
        var descuentoDecimal = (decimal)descuento / 100m;
        var totalConDescuento = subtotal * (1m - descuentoDecimal);
        var totalConRecargo = totalConDescuento * (1m + Recargo);
        return Math.Round(totalConRecargo, 2);
    }
}
