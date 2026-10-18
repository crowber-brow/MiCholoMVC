using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Models;
using MiCholoMVC.Services;
using MiCholoMVC.ViewModels;
namespace MiCholoMVC.Controllers;
public class PerdidasController(MiCholoDbContext db, OperacionService operaciones, ILogger<PerdidasController> logger) : Controller
{
    public async Task<IActionResult> Index(int? idProducto, string? motivo, DateTime? desde, DateTime? hasta)
    {
        var query=db.Perdidas.Include(p=>p.Lote!).ThenInclude(l=>l.Producto).AsQueryable();
        if(idProducto.HasValue) query=query.Where(p=>p.Lote!.IdProducto==idProducto);
        if(!string.IsNullOrWhiteSpace(motivo)) query=query.Where(p=>p.Motivo==motivo);
        if(desde.HasValue) query=query.Where(p=>p.Fecha>=desde.Value.Date);
        if(hasta.HasValue) query=query.Where(p=>p.Fecha<hasta.Value.Date.AddDays(1));
        await Cargar(idProducto); ViewBag.Motivo=motivo; ViewBag.Desde=desde; ViewBag.Hasta=hasta;
        return View(await query.OrderByDescending(p=>p.Fecha).ToListAsync());
    }
    [HttpGet] public async Task<IActionResult> Create() { await Cargar();return View(); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PerdidaCreateViewModel modelo)
    {
        if(!ReglasNegocio.MotivosPerdida.Contains(modelo.Motivo)) ModelState.AddModelError("Motivo","Seleccione un motivo válido.");
        if(string.IsNullOrWhiteSpace(modelo.Justificacion)) ModelState.AddModelError("Justificacion","La justificación es obligatoria.");
        if(ModelState.IsValid) try
        {
            await using var tx=await operaciones.IniciarAsync();
            var lote=await db.Lotes.SingleOrDefaultAsync(l=>l.IdLote==modelo.IdLote&&l.Estado)??throw new InvalidOperationException("El lote no existe o está inactivo.");
            if(lote.IdProducto!=modelo.IdProducto) throw new InvalidOperationException("El lote no pertenece al producto seleccionado.");
            ReglasNegocio.ValidarCantidad(modelo.Cantidad,lote.Cantidad);
            lote.Cantidad-=modelo.Cantidad;
            var perdida=new Perdida {IdLote=lote.IdLote,Cantidad=modelo.Cantidad,Motivo=modelo.Motivo,Justificacion=modelo.Justificacion.Trim(),Fecha=ReglasNegocio.Ahora,ValorUnitario=lote.CostoUnitario};
            db.Perdidas.Add(perdida); await db.SaveChangesAsync();
            await operaciones.MovimientoAsync(lote,"PERDIDA",modelo.Cantidad,$"Pérdida #{perdida.IdPerdida}: {modelo.Motivo}");
            await db.SaveChangesAsync();await tx.CommitAsync();TempData["Mensaje"]="Pérdida registrada y stock actualizado.";return RedirectToAction(nameof(Index));
        }
        catch(InvalidOperationException ex){ModelState.AddModelError("",ex.Message);}
        catch(Exception ex){logger.LogError(ex,"Error registrando pérdida");ModelState.AddModelError("","No se pudo registrar. Intente nuevamente.");}
        db.ChangeTracker.Clear();await Cargar(modelo.IdProducto,modelo.IdLote);return View(modelo);
    }
    public async Task<IActionResult> ObtenerLotesPorProducto(int idProducto)=>Json(await db.Lotes.Where(l=>l.IdProducto==idProducto&&l.Estado&&l.Cantidad>0).OrderBy(l=>l.FechaVencimiento).Select(l=>new{id=l.IdLote,numero=l.NumeroLote,cantidad=l.Cantidad,vencimiento=l.FechaVencimiento,costo=l.CostoUnitario}).ToListAsync());
    private async Task Cargar(int? producto=null,int? lote=null)
    {
        ViewBag.Productos=new SelectList(await db.Productos.OrderBy(p=>p.Nombre).ToListAsync(),"IdProducto","Nombre",producto);
        ViewBag.Lotes=new SelectList(await db.Lotes.Where(l=>l.IdProducto==producto&&l.Estado&&l.Cantidad>0).ToListAsync(),"IdLote","NumeroLote",lote);
    }
}
