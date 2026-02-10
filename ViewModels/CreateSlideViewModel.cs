using RHAds.Models.Areas;

namespace RHAds.ViewModels
{
    public class CreateSlideViewModel
    {
        public int AreaId { get; set; }
        public IEnumerable<Area> AreasInstitucionales { get; set; }
    }
}
