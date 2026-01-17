using RHAds.Models.Areas;

public class Slide
{
    public int SlideId { get; set; }

    public int AreaId { get; set; }
    public Area Area { get; set; }

    public string Titulo { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; }

    // NUEVO → Color de la cabecera del slide
    public string ColorCabecera { get; set; } = "#000000"; // valor por defecto

    public ICollection<SlideImage> SlideImages { get; set; }
}