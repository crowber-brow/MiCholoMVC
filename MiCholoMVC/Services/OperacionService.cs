using System.Data;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Models;
using MiCholoMVC.ViewModels;
namespace MiCholoMVC.Services;
public class OperacionService(MiCholoDbContext db)
{
    public async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> IniciarAsync()
    {
        var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            await db.Database.ExecuteSqlRawAsync("DECLARE @r int; EXEC @r = sys.sp_getapplock @Resource=N'MiCholoOperaciones', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000; IF @r < 0 THROW 51000, 'Otra operación está en curso. Inténtelo nuevamente.', 1;");
            return tx;
        }
        catch { await tx.DisposeAsync(); throw; }
    }
    public async Task MovimientoAsync(Lote lote, string tipo, int cantidad, string descripcion, int? saldoProducto = null)
    {
        var saldos = await db.Lotes.Where(l => l.IdProducto == lote.IdProducto).ToListAsync();
        db.MovimientosInventario.Add(new MovimientoInventario { IdProducto = lote.IdProducto, Lote = lote, Tipo = tipo, Cantidad = cantidad,
            SaldoLote = lote.Cantidad, SaldoProducto = saldoProducto ?? saldos.Sum(l => l.Cantidad), ValorUnitario = lote.CostoUnitario, Descripcion = descripcion, Fecha = ReglasNegocio.Ahora });
    }
    public async Task<int> VenderAsync(VentaCreateViewModel modelo)
    {
        await using var tx = await IniciarAsync();
        if (modelo.TokenOperacion == Guid.Empty) throw new InvalidOperationException("La operación no es válida. Recargue la pantalla.");
        var anterior = await db.Ventas.SingleOrDefaultAsync(v => v.TokenOperacion == modelo.TokenOperacion);
        if (anterior != null) return anterior.IdVenta;
        var caja = await db.Cajas.SingleOrDefaultAsync(c => c.Estado == "ABIERTA") ?? throw new InvalidOperationException("Debe abrir una caja antes de vender.");
        if (modelo.Detalles == null || modelo.Detalles.Count == 0) throw new InvalidOperationException("Agregue productos al carrito.");
        if (modelo.Detalles.Select(d => d.IdProducto).Distinct().Count() != modelo.Detalles.Count) throw new InvalidOperationException("Agrupe las cantidades de un mismo producto en una sola fila.");
        var venta = new Venta { Caja = caja, Fecha = ReglasNegocio.Ahora, MetodoPago = modelo.MetodoPago, TokenOperacion = modelo.TokenOperacion, Detalles = new List<DetalleVenta>() };
        foreach (var item in modelo.Detalles)
        {
            var producto = await db.Productos.Include(p => p.Categoria).SingleOrDefaultAsync(p => p.IdProducto == item.IdProducto && p.Estado) ?? throw new InvalidOperationException("Uno de los productos ya no está disponible.");
            var lotes = await db.Lotes.Where(l => l.IdProducto == item.IdProducto && l.Estado && l.Cantidad > 0 && l.FechaVencimiento >= ReglasNegocio.Hoy).OrderBy(l => l.FechaVencimiento).ThenBy(l => l.IdLote).ToListAsync();
            ReglasNegocio.ValidarCantidad(item.Cantidad, lotes.Sum(l => l.Cantidad));
            if (producto.PrecioVenta <= 0) throw new InvalidOperationException("El producto no tiene un precio de venta válido.");
            var pendiente = item.Cantidad;
            foreach (var lote in lotes)
            {
                if (pendiente == 0) break;
                var cantidad = Math.Min(pendiente, lote.Cantidad);
                venta.Detalles.Add(new DetalleVenta { Lote = lote, Cantidad = cantidad, PrecioUnitario = producto.PrecioVenta, CostoUnitario = lote.CostoUnitario, Subtotal = producto.PrecioVenta * cantidad,
                    ProductoNombre = producto.Nombre, CategoriaNombre = producto.Categoria?.Nombre ?? "Sin categoría", IdCategoriaHistorica = producto.IdCategoria });
                lote.Cantidad -= cantidad; pendiente -= cantidad;
            }
        }
        venta.Total = venta.Detalles.Sum(d => d.Subtotal);
        venta.Vuelto = ReglasNegocio.Vuelto(modelo.MetodoPago, venta.Total, modelo.DineroRecibido);
        venta.DineroRecibido = modelo.MetodoPago == "EFECTIVO" ? modelo.DineroRecibido : venta.Total;
        db.Ventas.Add(venta); await db.SaveChangesAsync();
        foreach (var grupo in venta.Detalles.GroupBy(d => d.Lote!.IdProducto))
        {
            var saldo = await db.Lotes.Where(l => l.IdProducto == grupo.Key).SumAsync(l => l.Cantidad) + grupo.Sum(d => d.Cantidad);
            foreach (var detalle in grupo) { saldo -= detalle.Cantidad; await MovimientoAsync(detalle.Lote!, "VENTA", detalle.Cantidad, $"Venta #{venta.IdVenta}", saldo); }
        }
        if (venta.MetodoPago == "EFECTIVO") db.MovimientosCaja.Add(new MovimientoCaja { IdCaja = caja.IdCaja, Tipo = "INGRESO", Monto = venta.Total, Descripcion = $"Venta #{venta.IdVenta}", Fecha = venta.Fecha });
        await db.SaveChangesAsync(); await tx.CommitAsync(); return venta.IdVenta;
    }
    public async Task AnularAsync(int id, string motivo, bool reponer)
    {
        if (string.IsNullOrWhiteSpace(motivo) || motivo.Length > 500) throw new InvalidOperationException("Indique un motivo de hasta 500 caracteres.");
        await using var tx = await IniciarAsync();
        var venta = await db.Ventas.Include(v => v.Detalles!).ThenInclude(d => d.Lote).SingleOrDefaultAsync(v => v.IdVenta == id) ?? throw new InvalidOperationException("La venta no existe.");
        if (venta.Estado != "VIGENTE") throw new InvalidOperationException("La venta ya fue anulada o devuelta.");
        var caja = await db.Cajas.SingleOrDefaultAsync(c => c.Estado == "ABIERTA") ?? throw new InvalidOperationException("Abra una caja para registrar la devolución.");
        if (venta.MetodoPago == "EFECTIVO")
        {
            if (await SaldoCajaAsync(caja) < venta.Total) throw new InvalidOperationException("No hay efectivo suficiente para devolver esta venta.");
            db.MovimientosCaja.Add(new MovimientoCaja { IdCaja = caja.IdCaja, Tipo = "EGRESO", Monto = venta.Total, Fecha = ReglasNegocio.Ahora, Descripcion = $"Devolución venta #{id}" });
        }
        if (reponer) foreach (var detalle in venta.Detalles!)
        {
            var lote = detalle.Lote!;
            if (!lote.Estado || !ReglasNegocio.Vendible(lote.FechaVencimiento)) throw new InvalidOperationException("No se puede reponer un lote vencido o inactivo. Registre la devolución sin reposición.");
            lote.Cantidad += detalle.Cantidad;
            await MovimientoAsync(lote, "DEVOLUCION", detalle.Cantidad, $"Devolución venta #{id}");
        }
        else foreach (var detalle in venta.Detalles!)
        {
            // El stock ya salió al vender: se valora la pérdida sin volver a descontarlo.
            db.Perdidas.Add(new Perdida { IdLote = detalle.IdLote, Cantidad = detalle.Cantidad, ValorUnitario = detalle.CostoUnitario,
                Motivo = "DEVOLUCION_NO_APTA", Justificacion = $"Devolución venta #{id}: {motivo.Trim()}"[..Math.Min(500, $"Devolución venta #{id}: {motivo.Trim()}".Length)], Fecha = ReglasNegocio.Ahora });
        }
        venta.Estado = "ANULADA"; venta.FechaAnulacion = ReglasNegocio.Ahora; venta.MotivoAnulacion = motivo.Trim(); venta.StockDevuelto = reponer;
        await db.SaveChangesAsync(); await tx.CommitAsync();
    }
    public async Task<decimal> SaldoCajaAsync(Caja caja) => caja.MontoInicial + (await db.MovimientosCaja.Where(m => m.IdCaja == caja.IdCaja).SumAsync(m => (decimal?)(m.Tipo == "INGRESO" ? m.Monto : -m.Monto)) ?? 0);
}
