using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Models;
using MiCholoMVC.Services;
namespace MiCholoMVC.Controllers;
public class LotesController(MiCholoDbContext db, OperacionService operaciones, ILogger<LotesController> logger):Controller
{
    public async Task<IActionResult> Index()=>View(await db.Lotes.Include(l=>l.Producto).OrderBy(l=>l.FechaVencimiento).ToListAsync());
    [HttpGet] public async Task<IActionResult> Create(){await Cargar();return View(new Lote {FechaIngreso=ReglasNegocio.Hoy,FechaVencimiento=ReglasNegocio.Hoy.AddMonths(1)});}
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("IdProducto,NumeroLote,Cantidad,CostoUnitario,FechaIngreso,FechaVencimiento")] Lote lote)
    {
        Validar(lote);
        if(ModelState.IsValid) try
        {
            await using var tx=await operaciones.IniciarAsync();
            var producto=await db.Productos.SingleOrDefaultAsync(p=>p.IdProducto==lote.IdProducto&&p.Estado)??throw new InvalidOperationException("Seleccione un producto activo.");
            if(await db.Lotes.AnyAsync(l=>l.IdProducto==lote.IdProducto&&l.NumeroLote==lote.NumeroLote.Trim())) throw new InvalidOperationException("Ese número de lote ya existe para el producto.");
            lote.IdLote=0;lote.NumeroLote=lote.NumeroLote.Trim();lote.Estado=true;
            if(lote.CostoUnitario==0) lote.CostoUnitario=producto.PrecioCompra;
            ReglasNegocio.ValidarMoneda(lote.CostoUnitario);
            db.Lotes.Add(lote);await db.SaveChangesAsync();
            await operaciones.MovimientoAsync(lote,"ENTRADA",lote.Cantidad,"Ingreso inicial del lote");
            await db.SaveChangesAsync();await tx.CommitAsync();return RedirectToAction(nameof(Index));
        }catch(InvalidOperationException ex){ModelState.AddModelError("",ex.Message);}
        catch(Exception ex){logger.LogError(ex,"Error creando lote");ModelState.AddModelError("","No se pudo guardar el lote.");}
        db.ChangeTracker.Clear();await Cargar(lote.IdProducto);return View(lote);
    }
    [HttpGet] public async Task<IActionResult> Edit(int id){var lote=await db.Lotes.FindAsync(id);if(lote==null)return NotFound();await Cargar(lote.IdProducto);return View(lote);}
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind("IdLote,IdProducto,NumeroLote,FechaIngreso,FechaVencimiento,Estado")] Lote lote)
    {
        Validar(lote);
        if(ModelState.IsValid) try
        {
            await using var tx=await operaciones.IniciarAsync();
            var actual=await db.Lotes.FindAsync(lote.IdLote);if(actual==null)return NotFound();
            if(actual.IdProducto!=lote.IdProducto) throw new InvalidOperationException("El producto del lote no puede cambiarse.");
            if(await db.Lotes.AnyAsync(l=>l.IdLote!=lote.IdLote&&l.IdProducto==lote.IdProducto&&l.NumeroLote==lote.NumeroLote.Trim())) throw new InvalidOperationException("Número de lote duplicado.");
            actual.NumeroLote=lote.NumeroLote.Trim();actual.FechaIngreso=lote.FechaIngreso;actual.FechaVencimiento=lote.FechaVencimiento;actual.Estado=lote.Estado;
            // Cantidad y costo se conservan: los cambios de stock pasan por movimientos.
            await db.SaveChangesAsync();await tx.CommitAsync();return RedirectToAction(nameof(Index));
        }catch(InvalidOperationException ex){ModelState.AddModelError("",ex.Message);}
        catch(Exception ex){logger.LogError(ex,"Error editando lote");ModelState.AddModelError("","No se pudo guardar el lote.");}
        db.ChangeTracker.Clear();await Cargar(lote.IdProducto);return View(lote);
    }
    private void Validar(Lote lote)
    {
        if(lote.Cantidad<0)ModelState.AddModelError("Cantidad","La cantidad no puede ser negativa.");
        if(lote.CostoUnitario<0)ModelState.AddModelError("CostoUnitario","El costo no puede ser negativo.");
        if(lote.FechaIngreso==default||lote.FechaVencimiento==default||lote.FechaVencimiento.Date<lote.FechaIngreso.Date)ModelState.AddModelError("FechaVencimiento","Revise las fechas de ingreso y vencimiento.");
    }
    private async Task Cargar(int? id=null)=>ViewBag.Productos=new SelectList(await db.Productos.OrderBy(p=>p.Nombre).ToListAsync(),"IdProducto","Nombre",id);
}
