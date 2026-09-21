using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.ViewModels;

namespace MiCholoMVC.Controllers
{
    public class ReportesController : Controller
    {
        private readonly MiCholoDbContext _context;

        public ReportesController(MiCholoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Ventas(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? idCaja,
            int? idCategoria)
        {
            var modelo = new ReporteVentasViewModel
            {
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                IdCaja = idCaja,
                IdCategoria = idCategoria
            };

            modelo.Cajas = await _context.Cajas
                .OrderBy(c => c.Nombre)
                .Select(c => new ReporteFiltroCajaViewModel
                {
                    IdCaja = c.IdCaja,
                    Nombre = c.Nombre
                })
                .ToListAsync();

            modelo.Categorias = await _context.Categorias
                .Where(c => c.Estado)
                .OrderBy(c => c.Nombre)
                .Select(c => new ReporteFiltroCategoriaViewModel
                {
                    IdCategoria = c.IdCategoria,
                    Nombre = c.Nombre
                })
                .ToListAsync();

            var consulta = _context.Ventas
                .AsQueryable();

            if (fechaDesde.HasValue)
            {
                var inicio = fechaDesde.Value.Date;

                consulta = consulta.Where(v =>
                    v.Fecha >= inicio);
            }

            if (fechaHasta.HasValue)
            {
                var fin = fechaHasta.Value.Date.AddDays(1);

                consulta = consulta.Where(v =>
                    v.Fecha < fin);
            }

            if (idCaja.HasValue)
            {
                consulta = consulta.Where(v =>
                    v.IdCaja == idCaja.Value);
            }

            var ventas = await consulta
                .Include(v => v.Caja)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            var idsVentas = ventas
                .Select(v => v.IdVenta)
                .ToList();

            var detalles = await _context.DetalleVentas
                .Where(d => idsVentas.Contains(d.IdVenta))
                .Join(
                    _context.Lotes,
                    d => d.IdLote,
                    l => l.IdLote,
                    (d, l) => new
                    {
                        d.IdVenta,
                        d.Cantidad,
                        l.IdProducto
                    })
                .Join(
                    _context.Productos,
                    x => x.IdProducto,
                    p => p.IdProducto,
                    (x, p) => new
                    {
                        x.IdVenta,
                        x.Cantidad,
                        p.IdCategoria,
                        Categoria = p.Categoria
                    })
                .ToListAsync();

            if (idCategoria.HasValue)
            {
                var idsVentasCategoria = detalles
                    .Where(d => d.IdCategoria == idCategoria.Value)
                    .Select(d => d.IdVenta)
                    .Distinct()
                    .ToHashSet();

                ventas = ventas
                    .Where(v => idsVentasCategoria.Contains(v.IdVenta))
                    .ToList();
            }

            modelo.Ventas = ventas
                .Select(v =>
                {
                    var detallesVenta = detalles
                        .Where(d => d.IdVenta == v.IdVenta)
                        .ToList();

                    var categorias = detallesVenta
                        .Where(d => d.Categoria != null)
                        .Select(d => d.Categoria!.Nombre)
                        .Distinct()
                        .ToList();

                    return new ReporteVentaDetalleViewModel
                    {
                        IdVenta = v.IdVenta,
                        Fecha = v.Fecha,
                        Caja = v.Caja?.Nombre ?? "Sin caja",
                        MetodoPago = v.MetodoPago,
                        Total = v.Total,
                        CantidadProductos = detallesVenta.Sum(d => d.Cantidad),
                        Categorias = categorias.Count > 0
                            ? string.Join(", ", categorias)
                            : "Sin categoría"
                    };
                })
                .ToList();

            modelo.TotalVentas = modelo.Ventas
                .Sum(v => v.Total);

            modelo.CantidadVentas = modelo.Ventas
                .Count;

            modelo.CantidadProductosVendidos = modelo.Ventas
                .Sum(v => v.CantidadProductos);

            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Perdidas(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            string? motivo)
        {
            var modelo = new ReportePerdidasViewModel
            {
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                Motivo = motivo
            };

            var consulta = _context.Perdidas
                .AsQueryable();

            if (fechaDesde.HasValue)
            {
                var inicio = fechaDesde.Value.Date;

                consulta = consulta.Where(p =>
                    p.Fecha >= inicio);
            }

            if (fechaHasta.HasValue)
            {
                var fin = fechaHasta.Value.Date.AddDays(1);

                consulta = consulta.Where(p =>
                    p.Fecha < fin);
            }

            if (!string.IsNullOrWhiteSpace(motivo))
            {
                consulta = consulta.Where(p =>
                    p.Motivo == motivo);
            }

            var perdidas = await consulta
                .Join(
                    _context.Lotes,
                    perdida => perdida.IdLote,
                    lote => lote.IdLote,
                    (perdida, lote) => new
                    {
                        perdida.IdPerdida,
                        perdida.Fecha,
                        perdida.Cantidad,
                        perdida.Motivo,
                        perdida.Justificacion,
                        lote.NumeroLote,
                        lote.IdProducto
                    })
                .Join(
                    _context.Productos,
                    x => x.IdProducto,
                    producto => producto.IdProducto,
                    (x, producto) => new
                    {
                        x.IdPerdida,
                        x.Fecha,
                        x.Cantidad,
                        x.Motivo,
                        x.Justificacion,
                        x.NumeroLote,
                        Producto = producto.Nombre,
                        producto.PrecioCompra
                    })
                .OrderByDescending(x => x.Fecha)
                .ToListAsync();

            modelo.Perdidas = perdidas
                .Select(p => new ReportePerdidaDetalleViewModel
                {
                    IdPerdida = p.IdPerdida,
                    Fecha = p.Fecha,
                    Producto = p.Producto,
                    NumeroLote = p.NumeroLote,
                    Cantidad = p.Cantidad,
                    Motivo = p.Motivo,
                    Justificacion = p.Justificacion,
                    PrecioCompra = p.PrecioCompra,
                    ValorEconomico =
                        p.Cantidad * p.PrecioCompra
                })
                .ToList();

            modelo.CantidadTotal = modelo.Perdidas
                .Sum(p => p.Cantidad);

            modelo.ValorEconomicoTotal = modelo.Perdidas
                .Sum(p => p.ValorEconomico);

            modelo.ResumenPorMotivo = modelo.Perdidas
                .GroupBy(p => p.Motivo)
                .Select(g => new ReportePerdidaResumenViewModel
                {
                    Motivo = g.Key,
                    Cantidad = g.Sum(p => p.Cantidad),
                    ValorEconomico = g.Sum(p => p.ValorEconomico)
                })
                .OrderByDescending(x => x.ValorEconomico)
                .ToList();

            return View(modelo);
        }
    }
}