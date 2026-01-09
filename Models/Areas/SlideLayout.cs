public class SlideLayout
{
    public int SlideLayoutId { get; set; }

    public int SlideId { get; set; }
    public Slide Slide { get; set; }

    public int AreaId { get; set; }
    public Area Area { get; set; }

    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public int Width { get; set; } = 4;
    public int Height { get; set; } = 2;
}