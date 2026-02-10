namespace RHAds.DTOs.Slides
{
    public class CreateSlideDto
    {
        public int AreaId { get; set; }
        public string Titulo { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
        public bool EsGlobal { get; set; }
        public string ColorCabecera { get; set; }
        public int? AreaDestinoId { get; set; }
    }
}