using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Services;
using MiCholoMVC.ViewModels;
namespace MiCholoMVC.Controllers;
public class ReportesController(MiCholoDbContext db):Controller
{
    public IActionResult Index()=>View();
    public Task<IActionResult> Ventas(DateTime? fechaDesde,DateTime? fechaHasta,int? idCaja,int? idCategoria)=>Informes("ventas",fechaDesde,fechaHasta,null,idCategoria,idCaja);
    public Task<IActionResult> Perdidas(DateTime? fechaDesde,DateTime? fechaHasta,string? motivo)=>Informes("perdidas",fechaDesde,fechaHasta,motivo:motivo);
    public async Task<IActionResult> Informes(string tipo="ventas",DateTime? desde=null,DateTime? hasta=null,int? idProducto=null,int? idCategoria=null,int? idCaja=null,string? motivo=null,string? grupo=null,bool incluirAnuladas=false,bool pdf=false)
    {
        if(tipo!="ventas"){idCategoria=null;incluirAnuladas=false;}
        if(tipo!="ventas"&&tipo!="caja")idCaja=null;
        if(tipo=="caja")idProducto=null;
        if(tipo!="perdidas")motivo=null;
        if(tipo!="vencimientos")grupo=null;
        var m=new InformeViewModel{Tipo=tipo,Desde=desde,Hasta=hasta,IdProducto=idProducto,IdCategoria=idCategoria,IdCaja=idCaja,Motivo=motivo,Grupo=grupo,IncluirAnuladas=incluirAnuladas};
        m.Productos=await db.Productos.OrderBy(p=>p.Nombre).Select(p=>new SelectListItem(p.Nombre,p.IdProducto.ToString())).ToListAsync();
        m.Categorias=await db.Categorias.OrderBy(c=>c.Nombre).Select(c=>new SelectListItem(c.Nombre,c.IdCategoria.ToString())).ToListAsync();
        m.Cajas=await db.Cajas.OrderByDescending(c=>c.IdCaja).Select(c=>new SelectListItem(c.Nombre+" #"+c.IdCaja,c.IdCaja.ToString())).ToListAsync();
        if(desde>hasta){ModelState.AddModelError("","La fecha inicial no puede superar la final.");return View("Informes",m);}
        string Mon(decimal x)=>"S/ "+x.ToString("F2");
        if(tipo=="ventas")
        {
            m.Titulo="Reporte de ventas";m.Columnas=["Venta","Fecha","Caja","Pago","Estado","Unidades","Importe"];
            var q=db.Ventas.Include(v=>v.Caja).Include(v=>v.Detalles!).ThenInclude(d=>d.Lote).AsQueryable();
            if(!incluirAnuladas)q=q.Where(v=>v.Estado=="VIGENTE");
            if(desde.HasValue)q=q.Where(v=>v.Fecha>=desde.Value.Date);if(hasta.HasValue)q=q.Where(v=>v.Fecha<hasta.Value.Date.AddDays(1));if(idCaja.HasValue)q=q.Where(v=>v.IdCaja==idCaja);
            decimal total=0,utilidad=0;int unidades=0,vigentes=0,anuladas=0;
            foreach(var v in await q.OrderByDescending(v=>v.Fecha).ToListAsync())
            {
                var detalles=v.Detalles!.Where(d=>(!idCategoria.HasValue||d.IdCategoriaHistorica==idCategoria)&&(!idProducto.HasValue||d.Lote!.IdProducto==idProducto)).ToList();
                if(detalles.Count==0)continue;var importe=detalles.Sum(d=>d.Subtotal);var cantidad=detalles.Sum(d=>d.Cantidad);
                if(v.Estado=="VIGENTE"){total+=importe;utilidad+=detalles.Sum(d=>d.Subtotal-d.CostoUnitario*d.Cantidad);unidades+=cantidad;vigentes++;}else anuladas++;
                m.Filas.Add([v.IdVenta.ToString(),v.Fecha.ToString("dd/MM/yyyy HH:mm"),v.Caja?.Nombre??"",v.MetodoPago,v.Estado,cantidad.ToString(),Mon(importe)]);
            }
            m.Totales=[$"Ventas vigentes: {vigentes} | Anuladas mostradas: {anuladas}",$"Unidades vigentes: {unidades} | Total vigente: {Mon(total)}",$"Utilidad bruta estimada vigente: {Mon(utilidad)}"];
        }
        else if(tipo=="perdidas")
        {
            m.Titulo="Reporte de pérdidas";m.Columnas=["Fecha","Producto","Lote","Motivo","Cantidad","Costo unit.","Valor perdido"];
            var q=db.Perdidas.Include(p=>p.Lote!).ThenInclude(l=>l.Producto).AsQueryable();
            if(desde.HasValue)q=q.Where(p=>p.Fecha>=desde.Value.Date);if(hasta.HasValue)q=q.Where(p=>p.Fecha<hasta.Value.Date.AddDays(1));if(idProducto.HasValue)q=q.Where(p=>p.Lote!.IdProducto==idProducto);if(!string.IsNullOrWhiteSpace(motivo))q=q.Where(p=>p.Motivo==motivo);
            var datos=await q.OrderByDescending(p=>p.Fecha).ToListAsync();
            foreach(var p in datos)m.Filas.Add([p.Fecha.ToString("dd/MM/yyyy"),p.Lote?.Producto?.Nombre??"",p.Lote?.NumeroLote??"",p.Motivo,p.Cantidad.ToString(),Mon(p.ValorUnitario),Mon(p.ValorTotal)]);
            m.Totales=[$"Registros: {datos.Count} | Unidades: {datos.Sum(p=>p.Cantidad)} | Valor perdido: {Mon(datos.Sum(p=>p.ValorTotal))}"];
            m.Totales.AddRange(datos.GroupBy(p=>p.Motivo).Select(g=>$"{g.Key}: {g.Sum(p=>p.Cantidad)} unidades | {Mon(g.Sum(p=>p.ValorTotal))}"));
        }
        else if(tipo=="inventario"||tipo=="stock")
        {
            m.Titulo=tipo=="stock"?"Reporte de stock mínimo":"Reporte de inventario";m.Columnas=["Producto","Lote","Físico","Disponible","Vencido","Mínimo","Costo unit.","Valor físico"];
            var q=db.Lotes.Include(l=>l.Producto).AsQueryable();if(idProducto.HasValue)q=q.Where(l=>l.IdProducto==idProducto);if(desde.HasValue)q=q.Where(l=>l.FechaIngreso>=desde.Value.Date);if(hasta.HasValue)q=q.Where(l=>l.FechaIngreso<hasta.Value.Date.AddDays(1));
            var datos=await q.OrderBy(l=>l.Producto!.Nombre).ToListAsync();
            var bajos=await db.Productos.Where(p=>p.Estado).Where(p=>(db.Lotes.Where(l=>l.IdProducto==p.IdProducto&&l.Estado&&l.FechaVencimiento>=ReglasNegocio.Hoy).Sum(l=>(int?)l.Cantidad)??0)<=p.StockMinimo).Select(p=>p.IdProducto).ToListAsync();
            if(tipo=="stock")datos=datos.Where(l=>bajos.Contains(l.IdProducto)).ToList();
            foreach(var l in datos)m.Filas.Add([l.Producto!.Nombre,l.NumeroLote,l.Cantidad.ToString(),(l.Estado&&ReglasNegocio.Vendible(l.FechaVencimiento)?l.Cantidad:0).ToString(),(!ReglasNegocio.Vendible(l.FechaVencimiento)?l.Cantidad:0).ToString(),l.Producto.StockMinimo.ToString(),Mon(l.CostoUnitario),Mon(l.Cantidad*l.CostoUnitario)]);
            if(tipo=="stock")foreach(var p in await db.Productos.Where(p=>bajos.Contains(p.IdProducto)&&!db.Lotes.Any(l=>l.IdProducto==p.IdProducto)&&(!idProducto.HasValue||p.IdProducto==idProducto)).ToListAsync())m.Filas.Add([p.Nombre,"Sin lotes","0","0","0",p.StockMinimo.ToString(),Mon(p.PrecioCompra),Mon(0)]);
            m.Totales=[$"Lotes: {datos.Count} | Unidades físicas: {datos.Sum(l=>l.Cantidad)} | Valor físico: {Mon(datos.Sum(l=>l.Cantidad*l.CostoUnitario))}"];
        }
        else if(tipo=="vencimientos")
        {
            m.Titulo="Reporte de vencimientos";m.Columnas=["Producto","Lote","Vencimiento","Estado","Cantidad","Valor al costo"];
            var q=db.Lotes.Where(l=>l.Estado&&l.Cantidad>0);if(idProducto.HasValue)q=q.Where(l=>l.IdProducto==idProducto);if(desde.HasValue)q=q.Where(l=>l.FechaVencimiento>=desde.Value.Date);if(hasta.HasValue)q=q.Where(l=>l.FechaVencimiento<hasta.Value.Date.AddDays(1));
            var datos=await q.OrderBy(l=>l.FechaVencimiento).Select(l=>new VencimientoViewModel{Producto=l.Producto!.Nombre,NumeroLote=l.NumeroLote,FechaVencimiento=l.FechaVencimiento,Cantidad=l.Cantidad,CostoUnitario=l.CostoUnitario}).ToListAsync();if(!string.IsNullOrWhiteSpace(grupo))datos=datos.Where(l=>l.Grupo==grupo).ToList();
            foreach(var l in datos)m.Filas.Add([l.Producto,l.NumeroLote,l.FechaVencimiento.ToString("dd/MM/yyyy"),l.EstadoVencimiento,l.Cantidad.ToString(),Mon(l.ValorTotal)]);
            m.Totales=[$"Lotes: {datos.Count} | Unidades: {datos.Sum(l=>l.Cantidad)} | Valor al costo: {Mon(datos.Sum(l=>l.ValorTotal))}"];
        }
        else if(tipo=="caja")
        {
            m.Titulo="Reporte de movimientos de caja";m.Columnas=["Fecha","Caja","Tipo","Monto","Descripción"];
            var q=db.MovimientosCaja.Include(x=>x.Caja).AsQueryable();if(idCaja.HasValue)q=q.Where(x=>x.IdCaja==idCaja);if(desde.HasValue)q=q.Where(x=>x.Fecha>=desde.Value.Date);if(hasta.HasValue)q=q.Where(x=>x.Fecha<hasta.Value.Date.AddDays(1));
            var datos=await q.OrderByDescending(x=>x.Fecha).ToListAsync();foreach(var x in datos)m.Filas.Add([x.Fecha.ToString("dd/MM/yyyy HH:mm"),x.Caja?.Nombre??"",x.Tipo,Mon(x.Monto),x.Descripcion??""]);
            var ingresos=datos.Where(x=>x.Tipo=="INGRESO").Sum(x=>x.Monto);var egresos=datos.Where(x=>x.Tipo=="EGRESO").Sum(x=>x.Monto);m.Totales=[$"Ingresos: {Mon(ingresos)} | Egresos: {Mon(egresos)} | Neto del período: {Mon(ingresos-egresos)}"];
        }
        else return BadRequest("Tipo de reporte inválido.");
        if(pdf)
        {
            var filtros=new List<string>{m.Periodo};
            if(idProducto.HasValue)filtros.Add("Producto: "+m.Productos.FirstOrDefault(p=>p.Value==idProducto.ToString())?.Text);
            if(idCategoria.HasValue)filtros.Add("Categoría: "+m.Categorias.FirstOrDefault(p=>p.Value==idCategoria.ToString())?.Text);
            if(idCaja.HasValue)filtros.Add("Caja: "+m.Cajas.FirstOrDefault(p=>p.Value==idCaja.ToString())?.Text);
            if(motivo!=null)filtros.Add("Motivo: "+motivo);if(grupo!=null)filtros.Add("Grupo: "+grupo);
            filtros.AddRange(m.Totales);filtros.Add(" ");
            if(m.Filas.Count==0)filtros.Add("No hay registros para los filtros seleccionados.");
            // Registros en bloques para que nombres largos no rompan columnas ni pierdan información.
            for(var i=0;i<m.Filas.Count;i++){filtros.Add($"Registro {i+1}");filtros.AddRange(m.Filas[i].Select((valor,j)=>$"{m.Columnas[j]}: {valor}"));filtros.Add(" ");}
            return File(PdfService.Crear(m.Titulo,filtros),"application/pdf",$"reporte-{tipo}.pdf");
        }
        return View("Informes",m);
    }
}
