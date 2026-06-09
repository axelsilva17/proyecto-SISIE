namespace proyecto_SISIE.Services.Strategy;

public interface IMetodoPagoStrategy
{
    int IdMetodoPago { get; }

    string NombreMetodoPago { get; }

    decimal CalcularTotal(decimal subtotal, int descuento);
}
