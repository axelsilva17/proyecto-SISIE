namespace proyecto_SISIE.Services.Strategy;

public class ProcesadorPago
{
    private readonly IEnumerable<IMetodoPagoStrategy> _estrategias;

    public ProcesadorPago(IEnumerable<IMetodoPagoStrategy> estrategias)
    {
        _estrategias = estrategias;
    }

    /// <summary>Selecciona la estrategia según el ID del método de pago y calcula el total.</summary>
    public decimal CalcularTotal(int idMetodoPago, decimal subtotal, int descuento)
    {
        var estrategia = _estrategias.FirstOrDefault(e =>
            e.IdMetodoPago == idMetodoPago);

        if (estrategia is null)
            throw new InvalidOperationException(
                $"No hay una estrategia registrada para el método de pago con ID '{idMetodoPago}'");

        return estrategia.CalcularTotal(subtotal, descuento);
    }
}
