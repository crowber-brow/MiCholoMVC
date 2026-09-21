using Microsoft.AspNetCore.Mvc;
using MiCholoMVC.Models;
using MiCholoMVC.Services;
using MiCholoMVC.ViewModels;

namespace MiCholoMVC.Controllers
{
    public class CajasController : Controller
    {
        private readonly ICajaService _service;

        public CajasController(ICajaService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var cajas = await _service.ObtenerTodasAsync();

            var saldos = new Dictionary<int, decimal>();

            foreach (var caja in cajas)
            {
                var movimientos =
                    await _service.ObtenerMovimientosAsync(caja.IdCaja);

                decimal saldo = caja.MontoInicial;

                foreach (var movimiento in movimientos)
                {
                    if (movimiento.Tipo == "INGRESO")
                    {
                        saldo += movimiento.Monto;
                    }
                    else if (movimiento.Tipo == "EGRESO")
                    {
                        saldo -= movimiento.Monto;
                    }
                }

                saldos[caja.IdCaja] = saldo;
            }

            ViewBag.Saldos = saldos;

            return View(cajas);
        }

        [HttpGet]
        public IActionResult Abrir()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Abrir(
            CajaAbrirViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var cajas = await _service.ObtenerTodasAsync();

            var cajaAbierta = cajas
                .FirstOrDefault(c => c.Estado == "ABIERTA");

            if (cajaAbierta != null)
            {
                ModelState.AddModelError(
                    "",
                    "Ya existe una caja abierta.");

                return View(modelo);
            }

            var caja = new Caja
            {
                Nombre = modelo.Nombre.Trim(),
                Estado = "ABIERTA",
                FechaApertura = DateTime.Now,
                MontoInicial = modelo.MontoInicial
            };

            await _service.CrearAsync(caja);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Movimientos(int id)
        {
            var caja = await _service.ObtenerPorIdAsync(id);

            if (caja == null)
            {
                return NotFound();
            }

            var movimientos =
                await _service.ObtenerMovimientosAsync(id);

            ViewBag.Caja = caja;

            return View(movimientos);
        }

        [HttpGet]
        public async Task<IActionResult> Movimiento(int id)
        {
            var caja = await _service.ObtenerPorIdAsync(id);

            if (caja == null)
            {
                return NotFound();
            }

            if (caja.Estado != "ABIERTA")
            {
                TempData["Error"] =
                    "No se pueden registrar movimientos en una caja cerrada.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Caja = caja;

            return View(new MovimientoCajaCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Movimiento(
            int id,
            MovimientoCajaCreateViewModel modelo)
        {
            var caja = await _service.ObtenerPorIdAsync(id);

            if (caja == null)
            {
                return NotFound();
            }

            if (caja.Estado != "ABIERTA")
            {
                TempData["Error"] =
                    "No se pueden registrar movimientos en una caja cerrada.";

                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Caja = caja;

                return View(modelo);
            }

            if (modelo.Tipo != "INGRESO" &&
                modelo.Tipo != "EGRESO")
            {
                ModelState.AddModelError(
                    "Tipo",
                    "El tipo de movimiento no es válido.");

                ViewBag.Caja = caja;

                return View(modelo);
            }

            if (modelo.Tipo == "EGRESO")
            {
                var movimientos =
                    await _service.ObtenerMovimientosAsync(id);

                decimal saldo = caja.MontoInicial;

                foreach (var movimiento in movimientos)
                {
                    if (movimiento.Tipo == "INGRESO")
                    {
                        saldo += movimiento.Monto;
                    }
                    else if (movimiento.Tipo == "EGRESO")
                    {
                        saldo -= movimiento.Monto;
                    }
                }

                if (modelo.Monto > saldo)
                {
                    ModelState.AddModelError(
                        "Monto",
                        $"El egreso no puede superar el saldo disponible de S/ {saldo:F2}.");

                    ViewBag.Caja = caja;

                    return View(modelo);
                }
            }

            var movimientoCaja = new MovimientoCaja
            {
                IdCaja = id,
                Tipo = modelo.Tipo,
                Monto = modelo.Monto,
                Descripcion = modelo.Descripcion.Trim(),
                Fecha = DateTime.Now
            };

            await _service.CrearMovimientoAsync(movimientoCaja);

            return RedirectToAction(
                nameof(Movimientos),
                new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Cerrar(int id)
        {
            var caja = await _service.ObtenerPorIdAsync(id);

            if (caja == null)
            {
                return NotFound();
            }

            if (caja.Estado != "ABIERTA")
            {
                TempData["Error"] =
                    "La caja ya está cerrada.";

                return RedirectToAction(nameof(Index));
            }

            var movimientos =
                await _service.ObtenerMovimientosAsync(id);

            decimal saldo = caja.MontoInicial;

            foreach (var movimiento in movimientos)
            {
                if (movimiento.Tipo == "INGRESO")
                {
                    saldo += movimiento.Monto;
                }
                else if (movimiento.Tipo == "EGRESO")
                {
                    saldo -= movimiento.Monto;
                }
            }

            ViewBag.SaldoFinal = saldo;

            return View(caja);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CerrarPost(int id)
        {
            var caja = await _service.ObtenerPorIdAsync(id);

            if (caja == null)
            {
                return NotFound();
            }

            if (caja.Estado != "ABIERTA")
            {
                TempData["Error"] =
                    "La caja ya está cerrada.";

                return RedirectToAction(nameof(Index));
            }

            var movimientos =
                await _service.ObtenerMovimientosAsync(id);

            decimal saldo = caja.MontoInicial;

            foreach (var movimiento in movimientos)
            {
                if (movimiento.Tipo == "INGRESO")
                {
                    saldo += movimiento.Monto;
                }
                else if (movimiento.Tipo == "EGRESO")
                {
                    saldo -= movimiento.Monto;
                }
            }

            caja.Estado = "CERRADA";
            caja.FechaCierre = DateTime.Now;
            caja.MontoCierre = saldo;

            await _service.ActualizarAsync(caja);

            TempData["Mensaje"] =
                $"Caja cerrada correctamente. Monto de cierre: S/ {saldo:F2}.";

            return RedirectToAction(nameof(Index));
        }
    }
}