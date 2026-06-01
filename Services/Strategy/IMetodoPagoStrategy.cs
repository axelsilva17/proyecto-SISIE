namespace proyecto_SISIE.Services.Strategy;

/// <summary>
/// Estrategia para calcular el total de una venta según el método de pago.
/// Cada método de pago puede aplicar recargos, descuentos o reglas distintas.
/// </summary>
public interface IMetodoPagoStrategy
{
 
    string MetodoPago { get; }

   
    decimal CalcularTotal(decimal subtotal, int descuento);
}
