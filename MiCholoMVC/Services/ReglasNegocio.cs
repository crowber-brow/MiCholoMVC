namespace MiCholoMVC.Services;
public static class ReglasNegocio
{
    public static readonly string[] MetodosPago = ["EFECTIVO", "YAPE", "PLIN", "TARJETA"];
    public static readonly string[] MotivosPerdida = ["ROBO", "ROTURA", "EXTRAVIO", "DETERIORO", "MERMA", "VENCIMIENTO", "DEVOLUCION_NO_APTA"];
    public static DateTime Ahora => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SA Pacific Standard Time");
    public static DateTime Hoy => Ahora.Date;
    // Se permite vender hasta el final de la fecha indicada de vencimiento.
    public static bool Vendible(DateTime fecha) => fecha.Date >= Hoy;
    public static decimal Vuelto(string metodo, decimal total, decimal recibido)
    {
        ValidarMoneda(total); ValidarMoneda(recibido, false);
        if (!MetodosPago.Contains(metodo)) throw new InvalidOperationException("Seleccione un método de pago válido.");
        if (total <= 0) throw new InvalidOperationException("La venta debe tener un importe mayor que cero.");
        if (metodo != "EFECTIVO") return 0;
        if (recibido < total) throw new InvalidOperationException("El dinero recibido no cubre el total de la venta.");
        return decimal.Round(recibido - total, 2);
    }
    public static void ValidarMoneda(decimal monto, bool positivo = true)
    {
        if (monto < 0 || (positivo && monto == 0) || monto > 99999999.99m || decimal.Round(monto,2) != monto)
            throw new InvalidOperationException("El monto debe estar dentro del rango permitido y tener como máximo dos decimales.");
    }
    public static void ValidarCantidad(int cantidad, int disponible)
    {
        if (cantidad <= 0) throw new InvalidOperationException("La cantidad debe ser mayor que cero.");
        if (cantidad > disponible) throw new InvalidOperationException("No hay stock suficiente.");
    }
}
