using RHAds.Models.Safety;

namespace RHAds.ViewModels
{
    public class SafetyBoardViewModel
    {
        public Dictionary<int, string> Colores { get; set; } = new();

        public int DiasDesdeIncidente { get; set; }
        public int DiasDesdeAccidente { get; set; }
        public int DiasDesdeNearMiss { get; set; }

        // Nuevos datos
        public SafetyEvent? UltimoIncidente { get; set; }
        public SafetyEvent? UltimoAccidente { get; set; }
        public SafetyEvent? UltimoNearMiss { get; set; }
    }
}