namespace RHAds.DTOs.Slides
{
    public class EditSlideDto
    {
        public int SlideId { get; set; }
        public string Titulo { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
        public string ColorCabecera { get; set; }
        public int? AreaDestinoId { get; set; }
        public bool EsGlobal { get; set; } = false;

    }
}
