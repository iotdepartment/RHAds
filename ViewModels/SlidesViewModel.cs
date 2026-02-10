using RHAds.Models.Areas;
using System.Collections.Generic;

namespace RHAds.ViewModels
{
    public class SlidesViewModel
    {
        public Area Area { get; set; }
        public IEnumerable<Area> AreasDestino { get; set; }
    }
}