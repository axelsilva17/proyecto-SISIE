namespace proyecto_SISIE.Services.Strategy;

public class EfectivoStrategy : IMetodoPagoStrategy
{
    public int IdMetodoPago => 1;
    public string NombreMetodoPago => "Efectivo";

    public decimal CalcularTotal(decimal subtotal, int descuento)
    {
        var descuentoDecimal = (decimal)descuento / 100m;
        return Math.Round(subtotal * (1m - descuentoDecimal), 2);
    }
}
