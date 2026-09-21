using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Models;
using MiCholoMVC.Services;

namespace MiCholoMVC.Controllers
{
    public class ProductosController : Controller
    {
        private readonly IProductoService _service;
        private readonly MiCholoDbContext _context;

        public ProductosController(
            IProductoService service,
            MiCholoDbContext context)
        {
            _service = service;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var productos = await _service.ObtenerTodasAsync();

            return View(productos);
        }

        public async Task<IActionResult> Create()
        {
            await CargarCombos();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (producto.PrecioCompra <= 0)
            {
                ModelState.AddModelError(
                    "PrecioCompra",
                    "El precio de compra debe ser mayor que 0.");
            }

            if (producto.PrecioVenta <= 0)
            {
                ModelState.AddModelError(
                    "PrecioVenta",
                    "El precio de venta debe ser mayor que 0.");
            }

            if (producto.PrecioVenta < producto.PrecioCompra)
            {
                ModelState.AddModelError(
                    "PrecioVenta",
                    "El precio de venta no puede ser menor que el precio de compra.");
            }

            if (producto.StockMinimo < 0)
            {
                ModelState.AddModelError(
                    "StockMinimo",
                    "El stock mínimo no puede ser negativo.");
            }

            if (!ModelState.IsValid)
            {
                await CargarCombos(producto.IdCategoria, producto.IdProveedor);

                return View(producto);
            }

            await _service.CrearAsync(producto);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var producto = await _service.ObtenerPorIdAsync(id);

            if (producto == null)
            {
                return NotFound();
            }

            await CargarCombos(
                producto.IdCategoria,
                producto.IdProveedor);

            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Producto producto)
        {
            if (producto.PrecioCompra <= 0)
            {
                ModelState.AddModelError(
                    "PrecioCompra",
                    "El precio de compra debe ser mayor que 0.");
            }

            if (producto.PrecioVenta <= 0)
            {
                ModelState.AddModelError(
                    "PrecioVenta",
                    "El precio de venta debe ser mayor que 0.");
            }

            if (producto.PrecioVenta < producto.PrecioCompra)
            {
                ModelState.AddModelError(
                    "PrecioVenta",
                    "El precio de venta no puede ser menor que el precio de compra.");
            }

            if (producto.StockMinimo < 0)
            {
                ModelState.AddModelError(
                    "StockMinimo",
                    "El stock mínimo no puede ser negativo.");
            }

            if (!ModelState.IsValid)
            {
                await CargarCombos(
                    producto.IdCategoria,
                    producto.IdProveedor);

                return View(producto);
            }

            var productoExistente =
                await _service.ObtenerPorIdAsync(producto.IdProducto);

            if (productoExistente == null)
            {
                return NotFound();
            }

            productoExistente.Nombre = producto.Nombre;
            productoExistente.Descripcion = producto.Descripcion;
            productoExistente.IdCategoria = producto.IdCategoria;
            productoExistente.IdProveedor = producto.IdProveedor;
            productoExistente.PrecioCompra = producto.PrecioCompra;
            productoExistente.PrecioVenta = producto.PrecioVenta;
            productoExistente.StockMinimo = producto.StockMinimo;
            productoExistente.Estado = producto.Estado;

            await _service.ActualizarAsync(productoExistente);

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCombos(
            int? categoriaSeleccionada = null,
            int? proveedorSeleccionado = null)
        {
            var categorias = await _context.Categorias
                .Where(c => c.Estado)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            var proveedores = await _context.Proveedores
                .Where(p => p.Estado)
                .OrderBy(p => p.RazonSocial)
                .ToListAsync();

            ViewBag.Categorias = new SelectList(
                categorias,
                "IdCategoria",
                "Nombre",
                categoriaSeleccionada);

            ViewBag.Proveedores = new SelectList(
                proveedores,
                "IdProveedor",
                "RazonSocial",
                proveedorSeleccionado);
        }
    }
}