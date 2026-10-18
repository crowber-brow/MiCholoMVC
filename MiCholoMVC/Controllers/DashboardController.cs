using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Services;
using MiCholoMVC.ViewModels;
namespace MiCholoMVC.Controllers;
public class DashboardController(MiCholoDbContext db,OperacionService operaciones):Controller
{
    public async Task<IActionResult> Index()
    {
        var hoy=ReglasNegocio.Hoy;var inicioMes=new DateTime(hoy.Year,hoy.Month,1);var inicioSeisMeses=inicioMes.AddMonths(-5);
        var ventas=db.Ventas.Where(v=>v.Estado=="VIGENTE");
        var recientes=await ventas.Where(v=>v.Fecha>=inicioSeisMeses).Select(v=>new{v.Fecha,v.Total}).ToListAsync();
        var m=new DashboardViewModel{VentasHoy=recientes.Where(v=>v.Fecha.Date==hoy).Sum(v=>v.Total),VentasMes=recientes.Where(v=>v.Fecha>=inicioMes).Sum(v=>v.Total),VentasTotales=await ventas.SumAsync(v=>(decimal?)v.Total)??0,CantidadVentas=await ventas.CountAsync()};
        var caja=await db.Cajas.SingleOrDefaultAsync(c=>c.Estado=="ABIERTA");m.CajaActual=caja==null?0:await operaciones.SaldoCajaAsync(caja);
        m.ProductosStockBajo=await db.Productos.Where(p=>p.Estado).CountAsync(p=>(db.Lotes.Where(l=>l.IdProducto==p.IdProducto&&l.Estado&&l.FechaVencimiento>=hoy).Sum(l=>(int?)l.Cantidad)??0)<=p.StockMinimo);
        var lotes=await db.Lotes.Where(l=>l.Estado&&l.Cantidad>0).Select(l=>new VencimientoViewModel{FechaVencimiento=l.FechaVencimiento,Cantidad=l.Cantidad,CostoUnitario=l.CostoUnitario}).ToListAsync();
        m.LotesVencidos=lotes.Count(l=>l.DiasRestantes<0);m.LotesPorVencer=lotes.Count(l=>l.DiasRestantes>=0&&l.DiasRestantes<=30);m.ValorPorVencer=lotes.Where(l=>l.DiasRestantes>=0&&l.DiasRestantes<=30).Sum(l=>l.ValorTotal);
        m.CantidadPerdidas=await db.Perdidas.CountAsync();m.TotalPerdidas=await db.Perdidas.SumAsync(p=>(decimal?)(p.Cantidad*p.ValorUnitario))??0;m.PerdidasMes=await db.Perdidas.Where(p=>p.Fecha>=inicioMes).SumAsync(p=>(decimal?)(p.Cantidad*p.ValorUnitario))??0;
        m.UtilidadBrutaMes=await db.DetalleVentas.Where(d=>d.Venta!.Estado=="VIGENTE"&&d.Venta.Fecha>=inicioMes).SumAsync(d=>(decimal?)(d.Subtotal-d.Cantidad*d.CostoUnitario))??0;
        m.VentasPorMetodoPago=await ventas.Where(v=>v.Fecha>=inicioMes).GroupBy(v=>v.MetodoPago).Select(g=>new VentaMetodoPagoViewModel{MetodoPago=g.Key,Total=g.Sum(v=>v.Total),Cantidad=g.Count()}).ToListAsync();
        for(var fecha=hoy.AddDays(-6);fecha<=hoy;fecha=fecha.AddDays(1))m.VentasUltimos7Dias.Add(new VentaDiaViewModel{Fecha=fecha,Total=recientes.Where(v=>v.Fecha.Date==fecha).Sum(v=>v.Total),Cantidad=recientes.Count(v=>v.Fecha.Date==fecha)});
        for(var fecha=inicioSeisMeses;fecha<=inicioMes;fecha=fecha.AddMonths(1))m.VentasPorMes.Add(new SerieDashboard{Nombre=fecha.ToString("MM/yyyy"),Valor=recientes.Where(v=>v.Fecha.Year==fecha.Year&&v.Fecha.Month==fecha.Month).Sum(v=>v.Total)});
        m.ProductosMasVendidos=await db.DetalleVentas.Where(d=>d.Venta!.Estado=="VIGENTE"&&d.Venta.Fecha>=inicioMes).GroupBy(d=>d.ProductoNombre).Select(g=>new SerieDashboard{Nombre=g.Key,Valor=g.Sum(d=>d.Cantidad)}).OrderByDescending(g=>g.Valor).Take(8).ToListAsync();
        m.PerdidasPorMotivo=await db.Perdidas.Where(p=>p.Fecha>=inicioMes).GroupBy(p=>p.Motivo).Select(g=>new SerieDashboard{Nombre=g.Key,Valor=g.Sum(p=>p.Cantidad*p.ValorUnitario)}).ToListAsync();
        m.VencimientosPorGrupo=lotes.GroupBy(l=>l.EstadoVencimiento).Select(g=>new SerieDashboard{Nombre=g.Key,Valor=g.Sum(l=>l.ValorTotal)}).ToList();
        return View(m);
    }
}
