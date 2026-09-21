using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.ViewModels;

namespace MiCholoMVC.Controllers
{
    public class DashboardController : Controller
    {
        private readonly MiCholoDbContext _context;

        public DashboardController(MiCholoDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;
            var mañana = hoy.AddDays(1);

            var inicio7Dias = hoy.AddDays(-6);

            var ventasHoy = await _context.Ventas
                .Where(v =>
                    v.Fecha >= hoy &&
                    v.Fecha < mañana)
                .SumAsync(v => (decimal?)v.Total) ?? 0;

            var ventasTotales = await _context.Ventas
                .SumAsync(v => (decimal?)v.Total) ?? 0;

            var cantidadVentas = await _context.Ventas
                .CountAsync();

            var productos = await _context.Productos
                .Where(p => p.Estado)
                .Select(p => new
                {
                    p.IdProducto,
                    p.StockMinimo,
                    StockActual = _context.Lotes
                        .Where(l =>
                            l.IdProducto == p.IdProducto &&
                            l.Estado)
                        .Sum(l => (int?)l.Cantidad) ?? 0
                })
                .ToListAsync();

            var productosStockBajo = productos
                .Count(p =>
                    p.StockActual <= p.StockMinimo);

            var lotesPorVencer = await _context.Lotes
                .CountAsync(l =>
                    l.Estado &&
                    l.FechaVencimiento >= hoy &&
                    l.FechaVencimiento <= hoy.AddDays(30));

            var lotesVencidos = await _context.Lotes
                .CountAsync(l =>
                    l.Estado &&
                    l.FechaVencimiento < hoy);

            var cantidadPerdidas = await _context.Perdidas
                .CountAsync();

            var totalPerdidas = await _context.Perdidas
                .Join(
                    _context.Lotes,
                    perdida => perdida.IdLote,
                    lote => lote.IdLote,
                    (perdida, lote) => new
                    {
                        perdida.Cantidad,
                        lote.IdProducto
                    })
                .Join(
                    _context.Productos,
                    x => x.IdProducto,
                    producto => producto.IdProducto,
                    (x, producto) =>
                        x.Cantidad * producto.PrecioCompra)
                .SumAsync(x => (decimal?)x) ?? 0;

            var ventasPorMetodoPago = await _context.Ventas
                .GroupBy(v => v.MetodoPago)
                .Select(g => new VentaMetodoPagoViewModel
                {
                    MetodoPago = g.Key,
                    Total = g.Sum(v => v.Total),
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            var ventasUltimos7Dias = new List<VentaDiaViewModel>();

            for (var fecha = inicio7Dias;
                 fecha <= hoy;
                 fecha = fecha.AddDays(1))
            {
                var fechaSiguiente = fecha.AddDays(1);

                var datos = await _context.Ventas
                    .Where(v =>
                        v.Fecha >= fecha &&
                        v.Fecha < fechaSiguiente)
                    .GroupBy(v => 1)
                    .Select(g => new
                    {
                        Total = g.Sum(v => v.Total),
                        Cantidad = g.Count()
                    })
                    .FirstOrDefaultAsync();

                ventasUltimos7Dias.Add(
                    new VentaDiaViewModel
                    {
                        Fecha = fecha,
                        Total = datos?.Total ?? 0,
                        Cantidad = datos?.Cantidad ?? 0
                    });
            }

            var modelo = new DashboardViewModel
            {
                VentasHoy = ventasHoy,
                VentasTotales = ventasTotales,
                ProductosStockBajo = productosStockBajo,
                LotesPorVencer = lotesPorVencer,
                LotesVencidos = lotesVencidos,
                TotalPerdidas = totalPerdidas,
                CantidadPerdidas = cantidadPerdidas,
                CantidadVentas = cantidadVentas,
                VentasPorMetodoPago = ventasPorMetodoPago,
                VentasUltimos7Dias = ventasUltimos7Dias
            };

            return View(modelo);
        }
    }
}