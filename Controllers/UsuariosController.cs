using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RHAds.Data;
using RHAds.Models.Usuarios;
using System.Security.Cryptography;
using System.Text;

namespace RHAds.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;
        public UsuariosController(AppDbContext context)
        {
            _context = context;

        }

        [HttpGet]
        public IActionResult IndexUsuarios()
        {
            var usuarios = _context.Usuarios.Include(u => u.Area).ToList();

            // Áreas desde BD
            ViewBag.Areas = _context.Areas.ToList();

            return View(usuarios);
        }

        [HttpGet]
        public IActionResult GetUsuario(int id)
        {
            var usuario = _context.Usuarios
                .Where(u => u.UsuarioId == id)
                .Select(u => new {
                    u.UsuarioId,
                    u.Nombre,
                    u.Email,
                    u.AreaId,
                    u.Rol
                    // 👈 No devolvemos PasswordHash ni nada relacionado
                })
                .FirstOrDefault();

            if (usuario == null)
                return NotFound(new { success = false, message = "Usuario no encontrado" });

            return Ok(usuario);
        }

        [HttpPost]
        public IActionResult SaveUsuario([FromForm] UsuarioViewModel model)
        {
            // Si quieres validar manualmente, hazlo aquí
            if (string.IsNullOrWhiteSpace(model.Nombre) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.PasswordPlain) ||
                model.AreaId <= 0 ||
                string.IsNullOrWhiteSpace(model.Rol))
            {
                return Ok(new { success = false, message = "Todos los campos son obligatorios" });
            }

            var usuario = new Usuario
            {
                UsuarioId = model.UsuarioId,
                Nombre = model.Nombre,
                Email = model.Email,
                Rol = model.Rol,
                AreaId = model.AreaId,
                PasswordHash = HashPassword(model.PasswordPlain)
            };

            if (usuario.UsuarioId == 0)
                _context.Usuarios.Add(usuario);
            else
                _context.Usuarios.Update(usuario);

            _context.SaveChanges();

            return Ok(new { success = true, message = "Usuario guardado correctamente" });
        }

        [HttpPost]
        public IActionResult DeleteUsuario(int id)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario == null)
                return Ok(new { success = false, message = "Usuario no encontrado" });

            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();

            return Ok(new { success = true, message = "Usuario eliminado correctamente" });
        }

        // 🔹 Método para generar hash seguro con SHA256
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        [HttpPost]
        public IActionResult EditUsuario([FromForm] UsuarioViewModel model)
        {
            // Eliminamos manualmente cualquier error de PasswordPlain
            ModelState.Remove(nameof(model.PasswordPlain));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Ok(new { success = false, message = string.Join(" | ", errors) });
            }

            var usuario = _context.Usuarios.Find(model.UsuarioId);

            if (usuario == null)
                return Ok(new { success = false, message = "Usuario no encontrado" });

            usuario.Nombre = model.Nombre;
            usuario.Email = model.Email;
            usuario.Rol = model.Rol;
            usuario.AreaId = model.AreaId;

            if (!string.IsNullOrWhiteSpace(model.PasswordPlain))
            {
                usuario.PasswordHash = HashPassword(model.PasswordPlain);
            }

            _context.Usuarios.Update(usuario);
            _context.SaveChanges();

            var areaNombre = _context.Areas.Find(usuario.AreaId)?.Nombre;

            return Ok(new
            {
                success = true,
                message = "Usuario actualizado correctamente",
                usuario = new
                {
                    usuario.UsuarioId,
                    usuario.Nombre,
                    usuario.Email,
                    usuario.Rol,
                    AreaNombre = areaNombre
                }
            });
        }

    }
}
