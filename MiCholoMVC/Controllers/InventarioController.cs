using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Models;
using MiCholoMVC.Services;

namespace MiCholoMVC.Controllers
{
    public class InventarioController : Controller
    {
        private readonly IMovimientoInventarioService _service;
        private readonly MiCholoDbContext _context;

        public InventarioController(
            IMovimientoInventarioService service,
            MiCholoDbContext context)
        {
            _service = service;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var movimientos =
                await _service.ObtenerTodosAsync();

            return View(movimientos);
        }

        public async Task<IActionResult> Create()
        {
            await CargarProductos();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MovimientoInventario movimiento)
        {
            if (movimiento.Cantidad <= 0)
            {
                ModelState.AddModelError(
                    "Cantidad",
                    "La cantidad debe ser mayor que 0.");
            }

            if (movimiento.Tipo != "ENTRADA" &&
                movimiento.Tipo != "SALIDA")
            {
                ModelState.AddModelError(
                    "Tipo",
                    "Seleccione un tipo de movimiento válido.");
            }

            var lote = await _context.Lotes
                .FirstOrDefaultAsync(
                    l => l.IdLote == movimiento.IdLote);

            if (lote == null)
            {
                ModelState.AddModelError(
                    "IdLote",
                    "El lote seleccionado no existe.");
            }
            else
            {
                if (lote.IdProducto != movimiento.IdProducto)
                {
                    ModelState.AddModelError(
                        "IdLote",
                        "El lote no pertenece al producto seleccionado.");
                }

                if (movimiento.Tipo == "SALIDA" &&
                    movimiento.Cantidad > lote.Cantidad)
                {
                    ModelState.AddModelError(
                        "Cantidad",
                        "No hay suficiente stock en el lote seleccionado.");
                }
            }

           

            if (!ModelState.IsValid)
            {

                await CargarProductos(
                    movimiento.IdProducto);

                await CargarLotes(
                    movimiento.IdProducto,
                    movimiento.IdLote);

                return View(movimiento);
            }

            if (movimiento.Tipo == "ENTRADA")
            {
                lote!.Cantidad += movimiento.Cantidad;
            }
            else
            {
                lote!.Cantidad -= movimiento.Cantidad;
            }

            await _service.CrearAsync(movimiento);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerLotesPorProducto(
    int idProducto)
        {
            var lotes = await _context.Lotes
                .Where(l =>
                    l.IdProducto == idProducto &&
                    l.Estado)
                .OrderBy(l => l.FechaVencimiento)
                .Select(l => new
                {
                    id = l.IdLote,
                    numero = l.NumeroLote,
                    cantidad = l.Cantidad,
                    vencimiento = l.FechaVencimiento
                })
                .ToListAsync();

            return Json(lotes);
        }

        private async Task CargarProductos(
            int? productoSeleccionado = null)
        {
            var productos = await _context.Productos
                .Where(p => p.Estado)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ViewBag.Productos = new SelectList(
                productos,
                "IdProducto",
                "Nombre",
                productoSeleccionado);
        }

        private async Task CargarLotes(
            int? productoId,
            int? loteSeleccionado = null)
        {
            if (productoId == null)
            {
                ViewBag.Lotes =
                    new SelectList(
                        new List<Lote>(),
                        "IdLote",
                        "NumeroLote");

                return;
            }

            var lotes = await _context.Lotes
                .Where(l =>
                    l.IdProducto == productoId &&
                    l.Estado)
                .OrderBy(l => l.FechaVencimiento)
                .ToListAsync();

            ViewBag.Lotes = new SelectList(
                lotes,
                "IdLote",
                "NumeroLote",
                loteSeleccionado);
        }
    }
}