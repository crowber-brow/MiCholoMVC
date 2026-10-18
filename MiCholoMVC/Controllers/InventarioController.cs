using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Models;
using MiCholoMVC.Services;
namespace MiCholoMVC.Controllers;
public class InventarioController(MiCholoDbContext db, OperacionService operaciones, ILogger<InventarioController> logger) : Controller
{
    public async Task<IActionResult> Index(int? idProducto, DateTime? desde, DateTime? hasta)
    {
        var inicio = await db.MovimientosInventario.Where(m => m.Tipo == "APERTURA_KARDEX").MinAsync(m => (DateTime?)m.Fecha);
        var query = db.MovimientosInventario.Include(m => m.Producto).Include(m => m.Lote).AsQueryable();
        if (inicio.HasValue) query = query.Where(m => m.Fecha >= inicio.Value);
        if (idProducto.HasValue) query = query.Where(m => m.IdProducto == idProducto);
        if (desde.HasValue) query = query.Where(m => m.Fecha >= desde.Value.Date);
        if (hasta.HasValue) query = query.Where(m => m.Fecha < hasta.Value.Date.AddDays(1));
        ViewBag.Inicio = inicio; ViewBag.Desde = desde; ViewBag.Hasta = hasta;
        await Cargar(idProducto); return View(await query.OrderByDescending(m => m.Fecha).ThenByDescending(m => m.IdMovimiento).ToListAsync());
    }
    [HttpGet] public async Task<IActionResult> Create() { await Cargar(); return View(); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MovimientoInventario movimiento)
    {
        if (movimiento.Tipo != "ENTRADA" && movimiento.Tipo != "SALIDA") ModelState.AddModelError("Tipo", "Seleccione ENTRADA o SALIDA.");
        if (string.IsNullOrWhiteSpace(movimiento.Descripcion)) ModelState.AddModelError("Descripcion", "Indique el motivo del movimiento.");
        if (ModelState.IsValid) try
        {
            await using var tx = await operaciones.IniciarAsync();
            var lote = await db.Lotes.SingleOrDefaultAsync(l => l.IdLote == movimiento.IdLote && l.Estado) ?? throw new InvalidOperationException("El lote no existe o está inactivo.");
            if (lote.IdProducto != movimiento.IdProducto) throw new InvalidOperationException("El lote no pertenece al producto seleccionado.");
            ReglasNegocio.ValidarCantidad(movimiento.Cantidad, movimiento.Tipo == "SALIDA" ? lote.Cantidad : int.MaxValue - lote.Cantidad);
            if (movimiento.Tipo == "ENTRADA" && !ReglasNegocio.Vendible(lote.FechaVencimiento)) throw new InvalidOperationException("No registre entradas a un lote vencido.");
            lote.Cantidad += movimiento.Tipo == "ENTRADA" ? movimiento.Cantidad : -movimiento.Cantidad;
            await operaciones.MovimientoAsync(lote,movimiento.Tipo,movimiento.Cantidad,movimiento.Descripcion!.Trim());
            await db.SaveChangesAsync(); await tx.CommitAsync(); TempData["Mensaje"]="Movimiento registrado."; return RedirectToAction(nameof(Index));
        }
        catch(InvalidOperationException ex) { ModelState.AddModelError("",ex.Message); }
        catch(Exception ex) { logger.LogError(ex,"Error de inventario"); ModelState.AddModelError("","No se pudo registrar. Intente nuevamente."); }
        db.ChangeTracker.Clear(); await Cargar(movimiento.IdProducto,movimiento.IdLote); return View(movimiento);
    }
    public async Task<IActionResult> ObtenerLotesPorProducto(int idProducto) => Json(await db.Lotes.Where(l=>l.IdProducto==idProducto&&l.Estado).OrderBy(l=>l.FechaVencimiento).Select(l=>new { id=l.IdLote,numero=l.NumeroLote,cantidad=l.Cantidad,vencimiento=l.FechaVencimiento }).ToListAsync());
    private async Task Cargar(int? producto=null,int? lote=null)
    {
        ViewBag.Productos=new SelectList(await db.Productos.OrderBy(p=>p.Nombre).ToListAsync(),"IdProducto","Nombre",producto);
        ViewBag.Lotes=new SelectList(await db.Lotes.Where(l=>l.IdProducto==producto&&l.Estado).ToListAsync(),"IdLote","NumeroLote",lote);
    }
}
