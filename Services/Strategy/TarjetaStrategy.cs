namespace proyecto_SISIE.Services.Strategy;

public class TarjetaStrategy : IMetodoPagoStrategy
{
    private const decimal Recargo = 0.03m;

    public int IdMetodoPago => 2;
    public string NombreMetodoPago => "Tarjeta";

    public decimal CalcularTotal(decimal subtotal, int descuento)
    {
        var descuentoDecimal = (decimal)descuento / 100m;
        var totalConDescuento = subtotal * (1m - descuentoDecimal);
        var totalConRecargo = totalConDescuento * (1m + Recargo);
        return Math.Round(totalConRecargo, 2);
    }
}
