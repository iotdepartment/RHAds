using RHAds.Models.Usuarios;
using RHAds.Models.Safety;

namespace RHAds.Models.Areas
{
    public class Area
    {
        public int AreaId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; } = true;

        // Flag para saber si el área tiene slide global
        public bool EsInstitucional { get; set; } = false;

        // Relaciones
        public ICollection<Usuario> Usuarios { get; set; }
        public ICollection<Slide> Slides { get; set; }
        public ICollection<SlideLayout> SlideLayouts { get; set; }

        // ✅ Relación con eventos de seguridad
        public ICollection<SafetyEvent> SafetyEvents { get; set; }
    }
}