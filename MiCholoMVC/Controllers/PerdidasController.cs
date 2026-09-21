using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Models;
using MiCholoMVC.Services;
using MiCholoMVC.ViewModels;

namespace MiCholoMVC.Controllers
{
    public class PerdidasController : Controller
    {
        private readonly IPerdidaService _service;
        private readonly MiCholoDbContext _context;

        public PerdidasController(
            IPerdidaService service,
            MiCholoDbContext context)
        {
            _service = service;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var perdidas =
                await _service.ObtenerTodosAsync();

            return View(perdidas);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarProductos();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PerdidaCreateViewModel modelo)
        {
            if (modelo.Cantidad <= 0)
            {
                ModelState.AddModelError(
                    "Cantidad",
                    "La cantidad debe ser mayor que 0.");
            }

            if (string.IsNullOrWhiteSpace(modelo.Justificacion))
            {
                ModelState.AddModelError(
                    "Justificacion",
                    "La justificación es obligatoria.");
            }

            var lote = await _context.Lotes
                .FirstOrDefaultAsync(
                    l => l.IdLote == modelo.IdLote);

            if (lote == null)
            {
                ModelState.AddModelError(
                    "IdLote",
                    "El lote seleccionado no existe.");
            }
            else
            {
                if (lote.IdProducto != modelo.IdProducto)
                {
                    ModelState.AddModelError(
                        "IdLote",
                        "El lote no pertenece al producto seleccionado.");
                }

                if (modelo.Cantidad > lote.Cantidad)
                {
                    ModelState.AddModelError(
                        "Cantidad",
                        "La cantidad perdida no puede ser mayor al stock disponible.");
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarProductos(
                    modelo.IdProducto);

                await CargarLotes(
                    modelo.IdProducto,
                    modelo.IdLote);

                return View(modelo);
            }

            lote!.Cantidad -= modelo.Cantidad;

            var perdida = new Perdida
            {
                IdLote = modelo.IdLote!.Value,
                Cantidad = modelo.Cantidad,
                Motivo = modelo.Motivo,
                Justificacion = modelo.Justificacion.Trim(),
                Fecha = DateTime.Now
            };

            _context.Perdidas.Add(perdida);

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
                ViewBag.Lotes = new SelectList(
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