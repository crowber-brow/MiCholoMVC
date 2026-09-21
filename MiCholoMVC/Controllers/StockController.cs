using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.ViewModels;

namespace MiCholoMVC.Controllers
{
    public class StockController : Controller
    {
        private readonly MiCholoDbContext _context;

        public StockController(MiCholoDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var stock = await _context.Productos
                .Where(p => p.Estado)
                .Select(p => new StockProductoViewModel
                {
                    IdProducto = p.IdProducto,
                    Nombre = p.Nombre,
                    StockActual = p.Lotes
                        .Where(l => l.Estado)
                        .Sum(l => l.Cantidad),
                    StockMinimo = p.StockMinimo
                })
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return View(stock);
        }
    }
}