using System.ComponentModel.DataAnnotations;
using RHAds.Models.Areas;

namespace RHAds.Models.Usuarios
{
    public class Usuario
    {
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        public string Rol { get; set; }

        [Required(ErrorMessage = "El área es obligatoria")]
        public int AreaId { get; set; }

        public Area Area { get; set; }
    }
}