using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RHAds.Data;

namespace RHAds.Controllers
{
    public class AreasController : Controller
    {
        private readonly AppDbContext _context;

        public AreasController(AppDbContext context)
        {
            _context = context;
        }

        // ============================
        //   VISTA PARA EL ÁREA RH
        // ============================
        public async Task<IActionResult> RH()
        {
            // Obtener el área RH
            var area = await _context.Areas
                .Include(a => a.Slides)
                    .ThenInclude(s => s.SlideImages)
                .FirstOrDefaultAsync(a => a.Nombre == "RH");

            if (area == null)
                return NotFound("El área RH no existe en la base de datos.");

            return View(area);
        }
    }
}