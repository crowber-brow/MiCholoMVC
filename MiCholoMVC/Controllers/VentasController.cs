using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Models;
using MiCholoMVC.ViewModels;

namespace MiCholoMVC.Controllers
{
    public class VentasController : Controller
    {
        private readonly MiCholoDbContext _context;

        public VentasController(MiCholoDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Caja)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            return View(ventas);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var cajaAbierta = await _context.Cajas
                .FirstOrDefaultAsync(c => c.Estado == "ABIERTA");

            if (cajaAbierta == null)
            {
                TempData["Error"] =
                    "No se puede registrar una venta porque no existe una caja abierta.";

                return RedirectToAction(nameof(Index));
            }

            var lotes = await _context.Lotes
                .Include(l => l.Producto)
                .Where(l =>
                    l.Estado &&
                    l.Cantidad > 0)
                .OrderBy(l => l.Producto.Nombre)
                .ThenBy(l => l.FechaVencimiento)
                .ToListAsync();

            ViewBag.Caja = cajaAbierta;
            ViewBag.Lotes = lotes;

            return View(new VentaCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(VentaCreateViewModel modelo)
        {
            var cajaAbierta = await _context.Cajas
                .FirstOrDefaultAsync(c => c.Estado == "ABIERTA");

            if (cajaAbierta == null)
            {
                TempData["Error"] =
                    "No se puede registrar una venta porque no existe una caja abierta.";

                return RedirectToAction(nameof(Index));
            }

            if (modelo.Detalles == null || modelo.Detalles.Count == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Debe agregar al menos un producto a la venta.");
            }

            if (!ModelState.IsValid)
            {
                await CargarDatosCrear(modelo, cajaAbierta.IdCaja);
                return View(modelo);
            }

            var idsLotes = modelo.Detalles
                .Select(d => d.IdLote)
                .ToList();

            if (idsLotes.Count != idsLotes.Distinct().Count())
            {
                ModelState.AddModelError(
                    "",
                    "No puede agregar el mismo lote más de una vez.");

                await CargarDatosCrear(modelo, cajaAbierta.IdCaja);
                return View(modelo);
            }

            var lotes = await _context.Lotes
                .Include(l => l.Producto)
                .Where(l => idsLotes.Contains(l.IdLote))
                .ToListAsync();

            if (lotes.Count != idsLotes.Count)
            {
                ModelState.AddModelError(
                    "",
                    "Uno o más lotes seleccionados no existen.");

                await CargarDatosCrear(modelo, cajaAbierta.IdCaja);
                return View(modelo);
            }

            foreach (var detalle in modelo.Detalles)
            {
                var lote = lotes.First(l => l.IdLote == detalle.IdLote);

                if (!lote.Estado)
                {
                    ModelState.AddModelError(
                        "",
                        $"El lote {lote.NumeroLote} está inactivo.");

                    continue;
                }

                if (lote.FechaVencimiento <= DateTime.Today)
                {
                    ModelState.AddModelError(
                        "",
                        $"El lote {lote.NumeroLote} está vencido y no puede venderse.");

                    continue;
                }

                if (detalle.Cantidad <= 0)
                {
                    ModelState.AddModelError(
                        "",
                        $"La cantidad del lote {lote.NumeroLote} debe ser mayor que 0.");

                    continue;
                }

                if (detalle.Cantidad > lote.Cantidad)
                {
                    ModelState.AddModelError(
                        "",
                        $"El lote {lote.NumeroLote} solo tiene {lote.Cantidad} unidades disponibles.");

                    continue;
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarDatosCrear(modelo, cajaAbierta.IdCaja);
                return View(modelo);
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var venta = new Venta
                {
                    IdCaja = cajaAbierta.IdCaja,
                    Fecha = DateTime.Now,
                    Total = 0,
                    MetodoPago = modelo.MetodoPago
                };

                _context.Ventas.Add(venta);

                await _context.SaveChangesAsync();

                decimal totalVenta = 0;

                foreach (var detalle in modelo.Detalles)
                {
                    var lote = lotes.First(l => l.IdLote == detalle.IdLote);

                    decimal precioUnitario =
                        lote.Producto.PrecioVenta;

                    decimal subtotal =
                        precioUnitario * detalle.Cantidad;

                    var detalleVenta = new DetalleVenta
                    {
                        IdVenta = venta.IdVenta,
                        IdLote = lote.IdLote,
                        Cantidad = detalle.Cantidad,
                        PrecioUnitario = precioUnitario,
                        Subtotal = subtotal
                    };

                    _context.DetalleVentas.Add(detalleVenta);

                    lote.Cantidad -= detalle.Cantidad;

                    totalVenta += subtotal;
                }

                venta.Total = totalVenta;

                var movimientoCaja = new MovimientoCaja
                {
                    IdCaja = cajaAbierta.IdCaja,
                    Tipo = "INGRESO",
                    Monto = totalVenta,
                    Descripcion = $"Venta #{venta.IdVenta}",
                    Fecha = DateTime.Now
                };

                _context.MovimientosCaja.Add(movimientoCaja);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Mensaje"] =
                    $"Venta #{venta.IdVenta} registrada correctamente. Total: S/ {totalVenta:F2}.";

                return RedirectToAction(
                    nameof(Detalle),
                    new { id = venta.IdVenta });
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al registrar la venta.");

                await CargarDatosCrear(modelo, cajaAbierta.IdCaja);

                return View(modelo);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Caja)
                .FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta == null)
            {
                return NotFound();
            }

            var detalles = await _context.DetalleVentas
                .Where(d => d.IdVenta == id)
                .Join(
                    _context.Lotes,
                    d => d.IdLote,
                    l => l.IdLote,
                    (d, l) => new
                    {
                        d.IdDetalleVenta,
                        d.IdLote,
                        d.Cantidad,
                        d.PrecioUnitario,
                        d.Subtotal,
                        l.NumeroLote,
                        l.FechaVencimiento,
                        l.IdProducto
                    })
                .Join(
                    _context.Productos,
                    d => d.IdProducto,
                    p => p.IdProducto,
                    (d, p) => new
                    {
                        d.IdDetalleVenta,
                        d.IdLote,
                        d.Cantidad,
                        d.PrecioUnitario,
                        d.Subtotal,
                        d.NumeroLote,
                        d.FechaVencimiento,
                        Producto = p.Nombre
                    })
                .ToListAsync();

            ViewBag.Detalles = detalles;

            return View(venta);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerLotes()
        {
            var lotes = await _context.Lotes
                .Include(l => l.Producto)
                .Where(l =>
                    l.Estado &&
                    l.Cantidad > 0 &&
                    l.FechaVencimiento > DateTime.Today)
                .OrderBy(l => l.Producto.Nombre)
                .ThenBy(l => l.FechaVencimiento)
                .Select(l => new
                {
                    idLote = l.IdLote,
                    producto = l.Producto.Nombre,
                    numeroLote = l.NumeroLote,
                    stock = l.Cantidad,
                    precio = l.Producto.PrecioVenta,
                    fechaVencimiento =
                        l.FechaVencimiento.ToString("yyyy-MM-dd")
                })
                .ToListAsync();

            return Json(lotes);
        }

        private async Task CargarDatosCrear(
            VentaCreateViewModel modelo,
            int idCaja)
        {
            var caja = await _context.Cajas
                .FirstOrDefaultAsync(c => c.IdCaja == idCaja);

            var lotes = await _context.Lotes
                .Include(l => l.Producto)
                .Where(l =>
                    l.Estado &&
                    l.Cantidad > 0)
                .OrderBy(l => l.Producto.Nombre)
                .ThenBy(l => l.FechaVencimiento)
                .ToListAsync();

            ViewBag.Caja = caja;
            ViewBag.Lotes = lotes;
        }
    }
}