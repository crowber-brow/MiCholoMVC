using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.ViewModels;

namespace MiCholoMVC.Controllers
{
    public class VencimientosController : Controller
    {
        private readonly MiCholoDbContext _context;

        public VencimientosController(
            MiCholoDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vencimientos = await _context.Lotes
                .Include(l => l.Producto)
                .Where(l => l.Estado)
                .Select(l => new VencimientoViewModel
                {
                    IdLote = l.IdLote,
                    Producto = l.Producto!.Nombre,
                    NumeroLote = l.NumeroLote,
                    Cantidad = l.Cantidad,
                    FechaVencimiento = l.FechaVencimiento
                })
                .OrderBy(l => l.FechaVencimiento)
                .ToListAsync();

            return View(vencimientos);
        }
    }
}