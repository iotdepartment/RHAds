public class SlideImage
{
    public int ImageId { get; set; }  
    public int SlideId { get; set; }
    public string RutaImagen { get; set; }
    public string Descripcion { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public Slide Slide { get; set; }
}