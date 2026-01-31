using RHAds.Models.Areas;

public class Slide
{
    public int SlideId { get; set; }
    public int AreaId { get; set; }
    public Area Area { get; set; }

    public string Titulo { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; }
    public string ColorCabecera { get; set; } = "#000000";
    public bool EsGlobal { get; set; } = false;

    public int? AreaDestinoId { get; set; }
    public Area AreaDestino { get; set; }
    public ICollection<SlideImage> SlideImages { get; set; }
    public ICollection<SlideLayout> SlideLayouts { get; set; }
}