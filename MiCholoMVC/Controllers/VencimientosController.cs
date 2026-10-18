using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.ViewModels;
namespace MiCholoMVC.Controllers;
public class VencimientosController(MiCholoDbContext db):Controller
{
    public async Task<IActionResult> Index(string? grupo,DateTime? desde,DateTime? hasta)
    {
        var query=db.Lotes.Where(l=>l.Estado&&l.Cantidad>0);
        if(desde.HasValue)query=query.Where(l=>l.FechaVencimiento>=desde.Value.Date);
        if(hasta.HasValue)query=query.Where(l=>l.FechaVencimiento<hasta.Value.Date.AddDays(1));
        var datos=await query.OrderBy(l=>l.FechaVencimiento).Select(l=>new VencimientoViewModel {IdLote=l.IdLote,Producto=l.Producto!.Nombre,NumeroLote=l.NumeroLote,Cantidad=l.Cantidad,FechaVencimiento=l.FechaVencimiento,CostoUnitario=l.CostoUnitario}).ToListAsync();
        ViewBag.Grupo=grupo;ViewBag.Desde=desde;ViewBag.Hasta=hasta;
        if(!string.IsNullOrWhiteSpace(grupo))datos=datos.Where(v=>v.Grupo==grupo).ToList();
        return View(datos);
    }
}
