using System.ComponentModel.DataAnnotations;

namespace RHAds.Models.Usuarios
{
    public class UsuarioViewModel
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string PasswordPlain { get; set; }
        public string Rol { get; set; }
        public int AreaId { get; set; }
    }
}