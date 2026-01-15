namespace RHAds.Models.Safety
{
    public class SafetyEvent
    {
        public int Id { get; set; }

        // Fecha exacta del evento
        public DateTime Fecha { get; set; }

        // Tipo de evento: 1=Near Miss, 2=Incidente, 3=Accidente
        public TipoEvento Tipo { get; set; }

        // Descripción opcional del evento
        public string? Descripcion { get; set; }

        // Área o departamento (opcional si manejas varias Safety Cross)
        public int? AreaId { get; set; }
    }

    public enum TipoEvento
    {
        NearMiss = 1,
        Incidente = 2,
        Accidente = 3
    }
}
