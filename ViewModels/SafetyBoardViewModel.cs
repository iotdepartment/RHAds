namespace RHAds.ViewModels
{
    public class SafetyBoardViewModel
    {
        public Dictionary<int, string> Colores { get; set; } = new();

        public int DiasDesdeIncidente { get; set; }
        public int DiasDesdeAccidente { get; set; }
        public int DiasDesdeNearMiss { get; set; }
    }
}