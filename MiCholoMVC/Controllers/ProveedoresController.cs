using Microsoft.AspNetCore.Mvc;
using MiCholoMVC.Models;
using MiCholoMVC.Services;

namespace MiCholoMVC.Controllers
{
    public class ProveedoresController : Controller
    {
        private readonly IProveedorService _service;

        public ProveedoresController(IProveedorService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var proveedores = await _service.ObtenerTodasAsync();

            return View(proveedores);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RazonSocial,Ruc,Telefono,Direccion,Estado")] Proveedor proveedor)
        {
            if (!ModelState.IsValid)
            {
                return View(proveedor);
            }

            await _service.CrearAsync(proveedor);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var proveedor = await _service.ObtenerPorIdAsync(id);

            if (proveedor == null)
            {
                return NotFound();
            }

            return View(proveedor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("IdProveedor,RazonSocial,Ruc,Telefono,Direccion,Estado")] Proveedor proveedor)
        {
            if (!ModelState.IsValid)
            {
                return View(proveedor);
            }

            await _service.ActualizarAsync(proveedor);

            return RedirectToAction(nameof(Index));
        }
    }
}
