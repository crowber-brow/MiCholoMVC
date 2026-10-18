using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Services;
using MiCholoMVC.ViewModels;
namespace MiCholoMVC.Controllers;
public class VentasController(MiCholoDbContext db, OperacionService operaciones, ILogger<VentasController> logger) : Controller
{
    public async Task<IActionResult> Index() => View(await db.Ventas.Include(v => v.Caja).OrderByDescending(v => v.Fecha).ToListAsync());
    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        if (!await db.Cajas.AnyAsync(c => c.Estado == "ABIERTA")) { TempData["Error"] = "Abra una caja antes de registrar ventas."; return RedirectToAction("Index", "Cajas"); }
        return View(new VentaCreateViewModel());
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(VentaCreateViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);
        try { return RedirectToAction(nameof(Detalle), new { id = await operaciones.VenderAsync(modelo) }); }
        catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "No se pudo registrar la venta"); ModelState.AddModelError("", "No se pudo registrar la venta. Revise la disponibilidad e intente nuevamente."); }
        db.ChangeTracker.Clear(); return View(modelo);
    }
    public async Task<IActionResult> ObtenerProductos(string? q)
    {
        var productos = db.Productos.Where(p => p.Estado);
        if (!string.IsNullOrWhiteSpace(q)) productos = productos.Where(p => p.Nombre.Contains(q));
        return Json(await productos.OrderBy(p => p.Nombre).Select(p => new { idProducto = p.IdProducto, nombre = p.Nombre, precio = p.PrecioVenta,
            stock = db.Lotes.Where(l => l.IdProducto == p.IdProducto && l.Estado && l.FechaVencimiento >= ReglasNegocio.Hoy).Sum(l => (int?)l.Cantidad) ?? 0 }).ToListAsync());
    }
    public async Task<IActionResult> Detalle(int id)
    {
        var venta = await db.Ventas.Include(v => v.Caja).Include(v => v.Detalles!).ThenInclude(d => d.Lote).SingleOrDefaultAsync(v => v.IdVenta == id);
        return venta == null ? NotFound() : View(venta);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(int id, string motivo, bool reponer)
    {
        try { await operaciones.AnularAsync(id, motivo, reponer); TempData["Mensaje"] = "Devolución registrada. Para pagos digitales, efectúe el reintegro por el medio de pago original."; }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        catch (Exception ex) { logger.LogError(ex, "Error al devolver venta {Id}", id); TempData["Error"] = "No se pudo registrar la devolución. Intente nuevamente."; }
        return RedirectToAction(nameof(Detalle), new { id });
    }
    public async Task<IActionResult> Comprobante(int id)
    {
        var venta = await db.Ventas.Include(v => v.Detalles).SingleOrDefaultAsync(v => v.IdVenta == id);
        if (venta == null) return NotFound();
        var lineas = new List<string> { $"Comprobante interno #{venta.IdVenta:D6}", $"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm} | Estado: {venta.Estado}", " " };
        lineas.AddRange(venta.Detalles!.GroupBy(d => new { d.ProductoNombre, d.PrecioUnitario }).Select(g => $"{g.Key.ProductoNombre} | {g.Sum(d => d.Cantidad)} x S/ {g.Key.PrecioUnitario:F2} | S/ {g.Sum(d => d.Subtotal):F2}"));
        lineas.AddRange([$"Total: S/ {venta.Total:F2}", $"Pago: {venta.MetodoPago}", $"Recibido: S/ {venta.DineroRecibido:F2} | Vuelto: S/ {venta.Vuelto:F2}", "Documento de control interno del minimarket."]);
        return File(PdfService.Crear("Mi Cholo - Comprobante", lineas), "application/pdf", $"venta-{id:D6}.pdf");
    }
}
