using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Models;
using MiCholoMVC.Services;

namespace MiCholoMVC.Controllers
{
    public class LotesController : Controller
    {
        private readonly ILoteService _service;
        private readonly MiCholoDbContext _context;

        public LotesController(
            ILoteService service,
            MiCholoDbContext context)
        {
            _service = service;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var lotes = await _service.ObtenerTodasAsync();

            return View(lotes);
        }

        public async Task<IActionResult> Create()
        {
            await CargarProductos();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lote lote)
        {
            if (lote.Cantidad < 0)
            {
                ModelState.AddModelError(
                    "Cantidad",
                    "La cantidad no puede ser negativa.");
            }

            if (lote.FechaVencimiento.Date < lote.FechaIngreso.Date)
            {
                ModelState.AddModelError(
                    "FechaVencimiento",
                    "La fecha de vencimiento no puede ser anterior a la fecha de ingreso.");
            }

            if (!ModelState.IsValid)
            {
                await CargarProductos(lote.IdProducto);

                return View(lote);
            }

            await _service.CrearAsync(lote);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var lote = await _service.ObtenerPorIdAsync(id);

            if (lote == null)
            {
                return NotFound();
            }

            await CargarProductos(lote.IdProducto);

            return View(lote);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Lote lote)
        {
            if (lote.Cantidad < 0)
            {
                ModelState.AddModelError(
                    "Cantidad",
                    "La cantidad no puede ser negativa.");
            }

            if (lote.FechaVencimiento.Date < lote.FechaIngreso.Date)
            {
                ModelState.AddModelError(
                    "FechaVencimiento",
                    "La fecha de vencimiento no puede ser anterior a la fecha de ingreso.");
            }

            if (!ModelState.IsValid)
            {
                await CargarProductos(lote.IdProducto);

                return View(lote);
            }

            var loteExistente = await _service.ObtenerPorIdAsync(lote.IdLote);

            if (loteExistente == null)
            {
                return NotFound();
            }

            loteExistente.IdProducto = lote.IdProducto;
            loteExistente.NumeroLote = lote.NumeroLote;
            loteExistente.Cantidad = lote.Cantidad;
            loteExistente.FechaIngreso = lote.FechaIngreso;
            loteExistente.FechaVencimiento = lote.FechaVencimiento;
            loteExistente.Estado = lote.Estado;

            await _service.ActualizarAsync(loteExistente);

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarProductos(int? productoSeleccionado = null)
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
    }
}