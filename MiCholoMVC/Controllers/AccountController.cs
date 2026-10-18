using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiCholoMVC.ViewModels;

namespace MiCholoMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly MiCholoMVC.Services.AccesoService _acceso;
        public AccountController(MiCholoMVC.Services.AccesoService acceso) { _acceso = acceso; }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!_acceso.Autenticar(model.Usuario, model.Password, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "local"))
            {
                ModelState.AddModelError(
                    "",
                    "Acceso no válido. Si hubo varios intentos fallidos, espere cinco minutos.");

                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim("VersionCredencial", _acceso.VersionCredencial()),
                new Claim(
                    ClaimTypes.Name,
                    "Administrador Mi Cholo"),

                new Claim(
                    ClaimTypes.Email,
                    "micholo@gmail.com"),

                new Claim(
                    ClaimTypes.Role,
                    "Administrador")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.Recordarme,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult Password() => View();
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Password(string actual, string nueva, string confirmar)
        {
            try {
                if (nueva != confirmar) throw new InvalidOperationException("Las contraseñas no coinciden.");
                _acceso.Cambiar(actual ?? "", nueva ?? "");
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["Mensaje"] = "Contraseña actualizada. Inicie sesión nuevamente.";
                return RedirectToAction("Login");
            } catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); return View(); }
        }
    }
}
