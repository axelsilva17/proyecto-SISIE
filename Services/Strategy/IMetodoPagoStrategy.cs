namespace proyecto_SISIE.Services.Strategy;

/// <summary>
/// Estrategia para calcular el total de una venta según el método de pago.
/// Cada método de pago puede aplicar recargos, descuentos o reglas distintas.
/// </summary>
public interface IMetodoPagoStrategy
{
    /// <summary>
    /// Identificador del método de pago (coincide con MetodoPago en Venta).
    /// </summary>
    string MetodoPago { get; }

    /// <summary>
    /// Calcula el total final aplicando las reglas del método de pago.
    /// </summary>
    /// <param name="subtotal">Suma de precio * cantidad de todos los productos.</param>
    /// <param name="descuento">Descuento en porcentaje entero (0-100).</param>
    /// <returns>Total final redondeado a 2 decimales.</returns>
    decimal CalcularTotal(decimal subtotal, int descuento);
}
