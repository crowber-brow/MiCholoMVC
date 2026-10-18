using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Models;
using MiCholoMVC.Services;
using MiCholoMVC.ViewModels;
namespace MiCholoMVC.Controllers;
public class CajasController(MiCholoDbContext db,OperacionService operaciones,ILogger<CajasController> logger):Controller
{
    public async Task<IActionResult> Index()
    {
        var cajas=await db.Cajas.OrderByDescending(c=>c.IdCaja).ToListAsync();
        var saldos=new Dictionary<int,decimal>();foreach(var caja in cajas)saldos[caja.IdCaja]=await operaciones.SaldoCajaAsync(caja);
        ViewBag.Saldos=saldos;return View(cajas);
    }
    [HttpGet] public IActionResult Abrir()=>View();
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Abrir(CajaAbrirViewModel modelo)
    {
        if(ModelState.IsValid)try{
            await using var tx=await operaciones.IniciarAsync();
            ReglasNegocio.ValidarMoneda(modelo.MontoInicial,false);
            if(await db.Cajas.AnyAsync(c=>c.Estado=="ABIERTA"))throw new InvalidOperationException("Ya existe una caja abierta.");
            db.Cajas.Add(new Caja {Nombre=modelo.Nombre.Trim(),MontoInicial=modelo.MontoInicial,Estado="ABIERTA",FechaApertura=ReglasNegocio.Ahora});
            await db.SaveChangesAsync();await tx.CommitAsync();return RedirectToAction(nameof(Index));
        }catch(InvalidOperationException ex){ModelState.AddModelError("",ex.Message);}
        catch(Exception ex){logger.LogError(ex,"Error abriendo caja");ModelState.AddModelError("","No se pudo abrir la caja.");}
        return View(modelo);
    }
    public async Task<IActionResult> Movimientos(int id)
    {
        var caja=await db.Cajas.FindAsync(id);if(caja==null)return NotFound();ViewBag.Caja=caja;
        ViewBag.Resumen=await db.Ventas.Where(v=>v.IdCaja==id&&v.Estado=="VIGENTE").GroupBy(v=>v.MetodoPago).Select(g=>new VentaMetodoPagoViewModel{MetodoPago=g.Key,Total=g.Sum(v=>v.Total),Cantidad=g.Count()}).ToListAsync();
        return View(await db.MovimientosCaja.Where(m=>m.IdCaja==id).OrderByDescending(m=>m.Fecha).ToListAsync());
    }
    [HttpGet] public async Task<IActionResult> Movimiento(int id)
    {
        var caja=await db.Cajas.FindAsync(id);if(caja==null)return NotFound();if(caja.Estado!="ABIERTA"){TempData["Error"]="La caja está cerrada.";return RedirectToAction(nameof(Index));}
        ViewBag.Caja=caja;return View(new MovimientoCajaCreateViewModel());
    }
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Movimiento(int id,MovimientoCajaCreateViewModel modelo)
    {
        if(ModelState.IsValid)try{
            await using var tx=await operaciones.IniciarAsync();
            var caja=await db.Cajas.FindAsync(id)??throw new InvalidOperationException("La caja no existe.");
            if(caja.Estado!="ABIERTA")throw new InvalidOperationException("No se permiten movimientos en una caja cerrada.");
            if(modelo.Tipo!="INGRESO"&&modelo.Tipo!="EGRESO")throw new InvalidOperationException("Tipo de movimiento inválido.");
            if(modelo.Monto<=0)throw new InvalidOperationException("El monto debe ser mayor que cero.");
            ReglasNegocio.ValidarMoneda(modelo.Monto);
            if(modelo.Tipo=="EGRESO"&&modelo.Monto>await operaciones.SaldoCajaAsync(caja))throw new InvalidOperationException("El egreso supera el efectivo disponible.");
            db.MovimientosCaja.Add(new MovimientoCaja{IdCaja=id,Tipo=modelo.Tipo,Monto=modelo.Monto,Descripcion=modelo.Descripcion.Trim(),Fecha=ReglasNegocio.Ahora});
            await db.SaveChangesAsync();await tx.CommitAsync();return RedirectToAction(nameof(Movimientos),new{id});
        }catch(InvalidOperationException ex){ModelState.AddModelError("",ex.Message);}
        catch(Exception ex){logger.LogError(ex,"Error movimiento de caja");ModelState.AddModelError("","No se pudo registrar el movimiento.");}
        db.ChangeTracker.Clear();ViewBag.Caja=await db.Cajas.FindAsync(id);return View(modelo);
    }
    [HttpGet] public async Task<IActionResult> Cerrar(int id)
    {
        var caja=await db.Cajas.FindAsync(id);if(caja==null)return NotFound();
        ViewBag.SaldoFinal=await operaciones.SaldoCajaAsync(caja);
        ViewBag.Resumen=await db.Ventas.Where(v=>v.IdCaja==id&&v.Estado=="VIGENTE").GroupBy(v=>v.MetodoPago).Select(g=>new VentaMetodoPagoViewModel{MetodoPago=g.Key,Total=g.Sum(v=>v.Total),Cantidad=g.Count()}).ToListAsync();
        return View(caja);
    }
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> CerrarPost(int id,decimal? efectivoContado)
    {
        try{
            if(!ModelState.IsValid||!efectivoContado.HasValue||efectivoContado<0)throw new InvalidOperationException("Ingrese un efectivo contado válido.");
            ReglasNegocio.ValidarMoneda(efectivoContado.Value,false);
            await using var tx=await operaciones.IniciarAsync();var caja=await db.Cajas.FindAsync(id)??throw new InvalidOperationException("La caja no existe.");
            if(caja.Estado!="ABIERTA")throw new InvalidOperationException("La caja ya está cerrada.");
            caja.MontoCierre=await operaciones.SaldoCajaAsync(caja);caja.EfectivoContado=efectivoContado;caja.Diferencia=efectivoContado-caja.MontoCierre;caja.Estado="CERRADA";caja.FechaCierre=ReglasNegocio.Ahora;
            await db.SaveChangesAsync();await tx.CommitAsync();TempData["Mensaje"]=$"Caja cerrada. Diferencia: S/ {caja.Diferencia:F2}.";return RedirectToAction(nameof(Index));
        }catch(InvalidOperationException ex){TempData["Error"]=ex.Message;}
        catch(Exception ex){logger.LogError(ex,"Error cerrando caja");TempData["Error"]="No se pudo cerrar la caja.";}
        return RedirectToAction(nameof(Cerrar),new{id});
    }
}
