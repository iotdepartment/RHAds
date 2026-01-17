using System.Collections.Generic;
using System.Drawing;


namespace RHAds.Models.Areas
{

    public class Area
    {
        public int AreaId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; } = true;

        public ICollection<Slide> Slides { get; set; }
        public ICollection<SlideLayout> SlideLayouts { get; set; }
    }

}