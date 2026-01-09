using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RHAds.Data;

namespace RHAds.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // PANEL
        public IActionResult Menu()
        {
            return View();
        }

        // LISTA DE ÁREAS
        public async Task<IActionResult> Areas()
        {
            var areas = await _context.Areas.ToListAsync();
            return View(areas);
        }

        // CREAR ÁREA (VISTA)
        public IActionResult CreateArea()
        {
            return View();
        }

        // CREAR ÁREA (FETCH)
        [HttpPost]
        public async Task<IActionResult> CreateAreaFetch([FromBody] Area model)
        {
            if (string.IsNullOrWhiteSpace(model.Nombre))
                return BadRequest(new { message = "El nombre es obligatorio." });

            _context.Areas.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Área creada correctamente." });
        }

        // LISTA DE SLIDES
        public async Task<IActionResult> Slides(int areaId)
        {
            var area = await _context.Areas
                .Include(a => a.Slides)
                .FirstOrDefaultAsync(a => a.AreaId == areaId);

            return View(area);
        }

        // CREAR SLIDE (VISTA)
        public IActionResult CreateSlide(int areaId)
        {
            ViewBag.AreaId = areaId;
            return View();
        }

        // CREAR SLIDE (FETCH)
        [HttpPost]
        public async Task<IActionResult> CreateSlideFetch([FromBody] Slide model)
        {
            if (string.IsNullOrWhiteSpace(model.Titulo))
                return BadRequest(new { message = "El título es obligatorio." });

            _context.Slides.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Slide creado correctamente." });
        }

        // LISTA DE IMÁGENES
        public async Task<IActionResult> Images(int slideId)
        {
            var slide = await _context.Slides
                .Include(s => s.SlideImages)
                .FirstOrDefaultAsync(s => s.SlideId == slideId);

            return View(slide);
        }

        // CREAR IMAGEN (VISTA)
        public IActionResult AddImage(int slideId)
        {
            ViewBag.SlideId = slideId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddImageUpload(IFormFile archivo, int slideId, string descripcion, int orden, bool activo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest(new { message = "No se seleccionó ninguna imagen." });

            // Crear carpeta si no existe
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/slides");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // Nombre único
            var fileName = $"slide_{slideId}_{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
            var filePath = Path.Combine(folder, fileName);

            // Guardar archivo físicamente
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // Guardar en BD
            var img = new SlideImage
            {
                SlideId = slideId,
                RutaImagen = "/images/slides/" + fileName,
                Descripcion = descripcion,
                Orden = orden,
                Activo = activo
            };

            _context.SlideImages.Add(img);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Imagen subida correctamente." });
        }

        [HttpGet]
        public IActionResult Fullscreen(int areaId)
        {
            var area = _context.Areas
                .Include(a => a.SlideLayouts)
                    .ThenInclude(sl => sl.Slide)
                        .ThenInclude(s => s.SlideImages)
                .FirstOrDefault(a => a.AreaId == areaId);

            return View(area);
        }

        [HttpGet]
        public IActionResult EditorLayout(int areaId)
        {
            var area = _context.Areas
                .Include(a => a.Slides)
                    .ThenInclude(s => s.SlideImages)
                .Include(a => a.SlideLayouts)
                    .ThenInclude(sl => sl.Slide)
                .FirstOrDefault(a => a.AreaId == areaId);

            if (area == null)
                return NotFound();

            return View("EditorLayout", area); // ← Nombre de la vista
        }

        [HttpPost]
        public IActionResult GuardarLayout(int areaId, IFormCollection form)
        {
            var layouts = _context.SlideLayouts
                .Where(l => l.AreaId == areaId)
                .ToList();

            foreach (var layout in layouts)
            {
                string x = form[$"X_{layout.SlideId}"];
                string y = form[$"Y_{layout.SlideId}"];
                string w = form[$"W_{layout.SlideId}"];
                string h = form[$"H_{layout.SlideId}"];

                // Si por alguna razón no llegaron valores, saltamos ese layout
                if (string.IsNullOrWhiteSpace(x) ||
                    string.IsNullOrWhiteSpace(y) ||
                    string.IsNullOrWhiteSpace(w) ||
                    string.IsNullOrWhiteSpace(h))
                {
                    continue;
                }

                if (int.TryParse(x, out var xVal)) layout.X = xVal;
                if (int.TryParse(y, out var yVal)) layout.Y = yVal;
                if (int.TryParse(w, out var wVal)) layout.Width = wVal;
                if (int.TryParse(h, out var hVal)) layout.Height = hVal;
            }

            _context.SaveChanges();

            return RedirectToAction("Fullscreen", new { areaId });
        }
    }
}