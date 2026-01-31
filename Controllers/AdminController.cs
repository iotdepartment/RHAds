using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RHAds.Data;
using RHAds.Helpers;
using RHAds.Models.Areas;


namespace RHAds.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;


        public AdminController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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

        // CREAR ÁREA (FETCH)
        [HttpPost]
        public async Task<IActionResult> CreateAreaFetch([FromBody] Area model)
        {
            if (string.IsNullOrWhiteSpace(model.Nombre))
                return BadRequest(new { message = "El nombre es obligatorio." });

            _context.Areas.Add(model);
            await _context.SaveChangesAsync();

            string html = await this.RenderViewAsync("Partials/_AreaRow", model, true);

            return Ok(new
            {
                message = "Área creada correctamente.",
                html = html
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetArea(int id)
        {
            var area = await _context.Areas.FirstOrDefaultAsync(x => x.AreaId == id);
            if (area == null) return NotFound();

            return Json(new { area.AreaId, area.Nombre, area.Descripcion });
        }

        [HttpPost]
        public async Task<IActionResult> EditArea(int areaId, string nombre, string descripcion)
        {
            var area = await _context.Areas.FirstOrDefaultAsync(x => x.AreaId == areaId);
            if (area == null) return BadRequest(new { message = "Área no encontrada." });

            area.Nombre = nombre;
            area.Descripcion = descripcion;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Área actualizada correctamente." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAreaActivo([FromBody] Area model)
        {
            var area = await _context.Areas.FirstOrDefaultAsync(x => x.AreaId == model.AreaId);
            if (area == null) return BadRequest(new { message = "Área no encontrada." });

            area.Activo = model.Activo;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Estado actualizado." });
        }
        [HttpPost]
        public IActionResult DeleteArea([FromBody] AreaDeleteRequest req)
        {
            var area = _context.Areas
                .Include(a => a.Slides)
                    .ThenInclude(s => s.SlideImages)
                .FirstOrDefault(a => a.AreaId == req.Id);

            if (area == null)
                return Ok(new { success = false, message = "Área no encontrada" });

            // Validar si hay usuarios asociados a esta área
            bool tieneUsuarios = _context.Usuarios.Any(u => u.AreaId == req.Id);

            if (tieneUsuarios)
                return Ok(new { success = false, message = "No se puede borrar el área porque tiene usuarios asociados" });

            // Eliminar archivos físicos de cada imagen de los slides
            foreach (var slide in area.Slides)
            {
                foreach (var image in slide.SlideImages)
                {
                    if (!string.IsNullOrEmpty(image.RutaImagen))
                    {
                        var fullPath = Path.Combine(_env.WebRootPath, "uploads", image.RutaImagen);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                }
            }

            // EF se encargará del cascade delete en la base para Slides e Imágenes
            _context.Areas.Remove(area);
            _context.SaveChanges();

            return Ok(new { success = true, message = "Área eliminada correctamente" });
        }


        public class AreaDeleteRequest
        {
            public int Id { get; set; }
        }


        // LISTA DE SLIDES
        public IActionResult Slides(int areaId)
        {
            var area = _context.Areas
                .Include(a => a.Slides)
                .FirstOrDefault(a => a.AreaId == areaId);

            if (area == null)
                return NotFound();

            ViewBag.Areas = _context.Areas.ToList(); // ← importante

            return View(area);
        }

        // CREAR SLIDE (VISTA)
        public IActionResult CreateSlide(int areaId)
        {
            ViewBag.AreaId = areaId;
            return View();
        }

        //FETCH CREAR SLIDE
        [HttpPost]
        public async Task<IActionResult> CreateSlideFetch([FromBody] Slide model)
        {
            if (string.IsNullOrWhiteSpace(model.Titulo))
                return BadRequest(new { message = "El título es obligatorio." });

            _context.Slides.Add(model);
            await _context.SaveChangesAsync();
            ViewBag.Areas = _context.Areas.ToList();

            // Solo crear layout si NO es global
            if (!model.EsGlobal)
            {
                var layout = new SlideLayout
                {
                    SlideId = model.SlideId,
                    AreaId = model.AreaId,
                    X = 0,
                    Y = 10,
                    Width = 2,
                    Height = 2
                };

                _context.SlideLayouts.Add(layout);
                await _context.SaveChangesAsync();
            }

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
        public async Task<IActionResult> AddImageUpload(IFormFile imagen, int slideId, string descripcion, int orden, bool activo)
        {
            if (imagen == null || imagen.Length == 0)
                return BadRequest(new { message = "No se recibió ninguna imagen." });

            // Crear carpeta si no existe
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/slides");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // Nombre único
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imagen.FileName)}";
            var filePath = Path.Combine(folder, fileName);

            // Guardar archivo
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }

            // Guardar en BD
            var slideImage = new SlideImage
            {
                SlideId = slideId,
                Descripcion = descripcion,
                Orden = orden,
                Activo = activo,
                RutaImagen = $"/uploads/slides/{fileName}"
            };

            _context.SlideImages.Add(slideImage);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Imagen agregada correctamente." });
        }

        [HttpGet]
        public async Task<IActionResult> GetImage(int id)
        {
            var img = await _context.SlideImages.FirstOrDefaultAsync(x => x.ImageId == id);

            if (img == null)
                return BadRequest(new { message = "Imagen no encontrada." });

            return Json(img);
        }

        [HttpPost]
        public async Task<IActionResult> EditImageUpload(IFormFile nuevaImagen, int imageId, string descripcion)
        {
            var img = await _context.SlideImages.FirstOrDefaultAsync(x => x.ImageId == imageId);

            if (img == null)
                return BadRequest(new { message = "Imagen no encontrada." });

            // Actualizar solo la descripción
            img.Descripcion = descripcion ?? "";

            // Si NO se subió una nueva imagen, solo guarda la descripción
            if (nuevaImagen == null || nuevaImagen.Length == 0)
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Imagen actualizada correctamente." });
            }

            // Carpeta donde se guardan las imágenes
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "slides");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // Eliminar imagen anterior si existe
            if (!string.IsNullOrEmpty(img.RutaImagen))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.RutaImagen.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            // Guardar nueva imagen
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(nuevaImagen.FileName)}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await nuevaImagen.CopyToAsync(stream);
            }

            img.RutaImagen = $"/images/slides/{fileName}";

            await _context.SaveChangesAsync();

            return Ok(new { message = "Imagen actualizada correctamente." });
        }

        public async Task<IActionResult> DeleteImageFetch(int id)
        {
            var img = await _context.SlideImages.FindAsync(id);
            if (img == null)
                return NotFound(new { message = "Imagen no encontrada." });

            _context.SlideImages.Remove(img);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Imagen eliminada correctamente." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateImageOrder([FromBody] List<SlideImage> orden)
        {
            foreach (var item in orden)
            {
                var img = await _context.SlideImages.FirstOrDefaultAsync(x => x.ImageId == item.ImageId);
                if (img != null)
                    img.Orden = item.Orden;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateImageActivo([FromBody] SlideImage model)
        {
            var img = await _context.SlideImages.FirstOrDefaultAsync(x => x.ImageId == model.ImageId);

            if (img == null)
                return BadRequest(new { message = "Imagen no encontrada." });

            img.Activo = model.Activo;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Estado actualizado." });
        }



        [HttpGet]
        public IActionResult Fullscreen(int areaId)
        {
            var area = _context.Areas
                .Include(a => a.SlideLayouts)
                    .ThenInclude(sl => sl.Slide)
                        .ThenInclude(s => s.SlideImages)
                .FirstOrDefault(a => a.AreaId == areaId);

            if (area == null)
                return NotFound();

            // Cargar slides globales (RH y EHS)
            var slidesGlobales = _context.Slides
                .Where(s => s.EsGlobal)
                .Include(s => s.SlideImages)
                .ToList();

            // Enviar a la vista
            ViewBag.SlidesGlobales = slidesGlobales;

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

            // Cargar slides globales (RH y EHS)
            var slidesGlobales = _context.Slides
                .Where(s => s.EsGlobal)
                .Include(s => s.SlideImages)
                .ToList();

            ViewBag.SlidesGlobales = slidesGlobales;

            return View("EditorLayout", area);
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

        [HttpPost]
        public IActionResult UpdateSlideColorAjax([FromBody] UpdateColorDto dto)
        {
            var slide = _context.Slides.FirstOrDefault(x => x.SlideId == dto.SlideId);
            if (slide == null)
                return NotFound(new { message = "Slide no encontrado" });

            slide.ColorCabecera = dto.Color;
            _context.SaveChanges();

            return Json(new { message = "Color actualizado", color = slide.ColorCabecera });
        }

        public class UpdateColorDto
        {
            public int SlideId { get; set; }
            public string Color { get; set; }
        }

        [HttpPost]
        public IActionResult ToggleSlideActivoAjax([FromBody] ToggleSlideActivoDto dto)
        {
            var slide = _context.Slides.FirstOrDefault(x => x.SlideId == dto.SlideId);
            if (slide == null)
                return NotFound();

            // Si se está reactivando
            if (dto.Activo && !slide.Activo)
            {
                var layout = _context.SlideLayouts.FirstOrDefault(x => x.SlideId == dto.SlideId);

                if (layout != null)
                {
                    // Resetear posición para evitar colisiones
                    layout.X = 0;
                    layout.Y = 10; // por ejemplo, última fila
                    layout.Width = 2;
                    layout.Height = 2;
                }
            }

            slide.Activo = dto.Activo;
            _context.SaveChanges();

            return Json(new { ok = true });
        }
        public class ToggleSlideActivoDto
        {
            public int SlideId { get; set; }
            public bool Activo { get; set; }
        }


    }
}