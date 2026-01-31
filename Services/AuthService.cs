using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RHAds.Data;
using RHAds.Models.Usuarios;

namespace RHAds.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public AuthService(AppDbContext context, IPasswordHasher<Usuario> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<Usuario?> AuthenticateAsync(string nombreUsuario, string password)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Area)
                .FirstOrDefaultAsync(u => u.Nombre == nombreUsuario);

            if (usuario == null)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password);

            if (result == PasswordVerificationResult.Failed)
                return null;

            return usuario;
        }
    }
}