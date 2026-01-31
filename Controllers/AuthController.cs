using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RHAds.Models.Usuarios;
using RHAds.Services;
using System.Security.Claims;

namespace RHAds.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }
        [AllowAnonymous]

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string nombreUsuario, string password)
        {
            var usuario = await _authService.AuthenticateAsync(nombreUsuario, password);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Usuario o contraseña inválidos");
                return View();
            }

            // Crear claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("AreaId", usuario.AreaId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Areas", "Admin");
        }
        [AllowAnonymous]

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }
    }
}