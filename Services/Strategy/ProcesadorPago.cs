namespace proyecto_SISIE.Services.Strategy;

public class ProcesadorPago
{
    private readonly IEnumerable<IMetodoPagoStrategy> _estrategias;

    public ProcesadorPago(IEnumerable<IMetodoPagoStrategy> estrategias)
    {
        _estrategias = estrategias;
    }


    /// Selecciona la estrategia adecuada según el método de pago y calcula el total.
    
    /// Total final redondeado a 2 decimales.

    public decimal CalcularTotal(string metodoPago, decimal subtotal, int descuento)
    {
        var estrategia = _estrategias.FirstOrDefault(e =>
            e.MetodoPago.Equals(metodoPago, StringComparison.OrdinalIgnoreCase));

        if (estrategia is null)
            throw new InvalidOperationException(
                $"No hay una estrategia registrada para el método de pago '{metodoPago}'");

        return estrategia.CalcularTotal(subtotal, descuento);
    }
}
