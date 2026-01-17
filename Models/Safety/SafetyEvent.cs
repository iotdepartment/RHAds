
using RHAds.Models.Areas;

namespace RHAds.Models.Safety
{
    public class SafetyEvent
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public TipoEvento Tipo { get; set; }
        public string? Descripcion { get; set; }

        public int? AreaId { get; set; }
        public Area? Area { get; set; }
    }

    public enum TipoEvento
    {
        NearMiss = 1,
        Incidente = 2,
        Accidente = 3
    }
}
