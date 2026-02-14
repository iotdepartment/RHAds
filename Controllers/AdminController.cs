using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RHAds.Data;
using RHAds.DTOs.Slides;
using RHAds.Helpers;
using RHAds.Models.Areas;
using RHAds.Services;
using RHAds.ViewModels;


namespace RHAds.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly SafetyService _safetyService;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;


    public AdminController(AppDbContext context, IWebHostEnvironment env)
    {
        _safetyService = new SafetyService(context);
        _context = context;
        _env = env;
    }

        // PANEL
        public IActionResult Menu()
        {
            return View();
        }


        private (int X, int Y) BuscarEspacioLibre(int areaId, int defaultX, int defaultY, int w, int h, int maxCols = 12, int maxRows = 12)
        {
            int x = defaultX;
            int y = defaultY;

            while (true)
            {
                bool ocupado = _context.SlideLayouts
                    .Any(l => l.AreaId == areaId &&
                              !(x + w <= l.X || l.X + l.Width <= x || y + h <= l.Y || l.Y + l.Height <= y));

                if (!ocupado)
                    return (x, y);

                // mover en eje X
                x += w;
                if (x + w > maxCols)
                {
                    x = 0;
                    y += h;
                }

                if (y + h > maxRows)
                    throw new InvalidOperationException("No hay espacio disponible en el grid.");
            }
        }


        // Vista principal de Áreas
        public IActionResult Areas()
        {
            return View();
        }

        // API para DataTables
        [HttpGet]
        public async Task<IActionResult> GetAreas()
        {
            var areas = await _context.Areas
                .Select(a => new {
                    areaId = a.AreaId,
                    nombre = a.Nombre,
                    descripcion = a.Descripcion,
                    activo = a.Activo,
                    esInstitucional = a.EsInstitucional
                })
                .ToListAsync();

            return Ok(new { data = areas });
        }

        // Obtener un área puntual
        [HttpGet]
        public async Task<IActionResult> GetArea(int id)
        {
            var area = await _context.Areas.FirstOrDefaultAsync(x => x.AreaId == id);
            if (area == null)
                return Ok(new { success = false, message = "Área no encontrada" });

            return Ok(new
            {
                success = true,
                areaId = area.AreaId,
                nombre = area.Nombre,
                descripcion = area.Descripcion,
                activo = area.Activo,
                esInstitucional = area.EsInstitucional
            });

        }

        // Crear área con slide global si es institucional
        [HttpPost]
        public async Task<IActionResult> CreateAreaFetch([FromBody] Area model)
        {
            if (string.IsNullOrWhiteSpace(model.Nombre))
                return BadRequest(new { success = false, message = "El nombre es obligatorio." });

            // ✅ Contar cuántos slides globales existen en total
            int totalGlobales = await _context.Slides.CountAsync(s => s.EsGlobal);

            // ✅ Si se intenta crear institucional y ya hay 2 → bloquear
            if (model.EsInstitucional && totalGlobales >= 2)
                return BadRequest(new { success = false, message = "Ya existen 2 áreas institucionales con slide global. No se pueden crear más." });

            _context.Areas.Add(model);
            await _context.SaveChangesAsync();

            if (model.EsInstitucional)
            {
                var slide = new Slide
                {
                    AreaId = model.AreaId,
                    Titulo = $"Slide Global {model.Nombre}",
                    Orden = await _context.Slides.MaxAsync(s => (int?)s.Orden) + 1 ?? 1,
                    Activo = true,
                    EsGlobal = true,
                    ColorCabecera = "#000000",
                    AreaDestinoId = model.AreaId
                };
                _context.Slides.Add(slide);
                await _context.SaveChangesAsync();

                // ✅ Buscar espacio libre
                var (x, y) = BuscarEspacioLibre(model.AreaId, 0, 0, 2, 2);

                var layout = new SlideLayout
                {
                    SlideId = slide.SlideId,
                    AreaId = model.AreaId,
                    X = x,
                    Y = y,
                    Width = 2,
                    Height = 2
                };

                _context.SlideLayouts.Add(layout);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                success = true,
                message = "Área creada correctamente.",
                data = new
                {
                    areaId = model.AreaId,
                    nombre = model.Nombre,
                    descripcion = model.Descripcion,
                    activo = model.Activo,
                    esInstitucional = model.EsInstitucional
                }
            });
        }

        // Editar área
        [HttpPost]
        public async Task<IActionResult> EditArea([FromBody] AreaEditRequest req)
        {
            var area = await _context.Areas
                .Include(a => a.Slides)
                .FirstOrDefaultAsync(x => x.AreaId == req.AreaId);

            if (area == null)
                return BadRequest(new { success = false, message = "Área no encontrada." });

            // ✅ Contar cuántos slides globales existen en total
            int totalGlobales = await _context.Slides.CountAsync(s => s.EsGlobal);

            // ✅ Si se intenta marcar como institucional y ya hay 2 globales → bloquear
            if (req.EsInstitucional && !area.EsInstitucional && totalGlobales >= 2)
                return BadRequest(new { success = false, message = "Ya existen 2 áreas institucionales con slide global. No se pueden crear más." });

            area.Nombre = req.Nombre;
            area.Descripcion = req.Descripcion;
            area.Activo = req.Activo;
            area.EsInstitucional = req.EsInstitucional;

            await _context.SaveChangesAsync();

            // ✅ Si es institucional y no tiene slide global, crear uno
            if (area.EsInstitucional && !area.Slides.Any(s => s.EsGlobal))
            {
                var slide = new Slide
                {
                    AreaId = area.AreaId,
                    Titulo = "Slide Institucional",
                    Orden = await _context.Slides.MaxAsync(s => (int?)s.Orden) + 1 ?? 1,
                    Activo = true,
                    EsGlobal = true,
                    ColorCabecera = "#000000",
                    AreaDestinoId = area.AreaId
                };

                _context.Slides.Add(slide);
                await _context.SaveChangesAsync();

                var (x, y) = BuscarEspacioLibre(area.AreaId, 0, 0, 2, 2);

                var layout = new SlideLayout
                {
                    SlideId = slide.SlideId,
                    AreaId = area.AreaId,
                    X = x,
                    Y = y,
                    Width = 2,
                    Height = 2
                };

                _context.SlideLayouts.Add(layout);
                await _context.SaveChangesAsync();
            }

            // ✅ Si ya no es institucional, eliminar el slide global
            if (!area.EsInstitucional && area.Slides.Any(s => s.EsGlobal))
            {
                var globalSlides = area.Slides.Where(s => s.EsGlobal).ToList();

                foreach (var slide in globalSlides)
                {
                    var layouts = _context.SlideLayouts.Where(l => l.SlideId == slide.SlideId);
                    _context.SlideLayouts.RemoveRange(layouts);

                    _context.Slides.Remove(slide);
                }

                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                success = true,
                message = "Área actualizada correctamente.",
                data = new
                {
                    areaId = area.AreaId,
                    nombre = area.Nombre,
                    descripcion = area.Descripcion,
                    activo = area.Activo,
                    esInstitucional = area.EsInstitucional
                }
            });
        }

        // Actualizar estado activo
        [HttpPost]
        public async Task<IActionResult> UpdateAreaActivo([FromBody] Area model)
        {
            var area = await _context.Areas.FirstOrDefaultAsync(x => x.AreaId == model.AreaId);
            if (area == null) return BadRequest(new { success = false, message = "Área no encontrada." });

            area.Activo = model.Activo;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Estado actualizado." });
        }

        // Eliminar área
        [HttpPost]
        public IActionResult DeleteArea([FromBody] AreaDeleteRequest req)
        {
            var area = _context.Areas
                .Include(a => a.Slides)
                    .ThenInclude(s => s.SlideImages)
                .FirstOrDefault(a => a.AreaId == req.Id);

            if (area == null)
                return Ok(new { success = false, message = "Área no encontrada" });

            bool tieneUsuarios = _context.Usuarios.Any(u => u.AreaId == req.Id);
            if (tieneUsuarios)
                return Ok(new { success = false, message = "No se puede borrar el área porque tiene usuarios asociados" });

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

            _context.Areas.Remove(area);
            _context.SaveChanges();

            return Ok(new { success = true, message = "Área eliminada correctamente" });
        }

        //  DTOs auxiliares
        public class AreaDeleteRequest
        {
            public int Id { get; set; }
        }

        public class AreaEditRequest
        {
            public int AreaId { get; set; }
            public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public bool Activo { get; set; }
            public bool EsInstitucional { get; set; }
        }



        // LISTA DE SLIDES
        public async Task<IActionResult> Slides(int areaId)
        {
            var area = await _context.Areas
                .Include(a => a.Slides)
                .FirstOrDefaultAsync(a => a.AreaId == areaId);

            if (area == null) return NotFound();

            var vm = new SlidesViewModel
            {
                Area = area,
                AreasDestino = await _context.Areas.ToListAsync()
            };

            ViewBag.Areas = await _context.Areas
                .Where(a => a.EsInstitucional == true)
                .ToListAsync();

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetSlides(int areaId)
        {
            var slides = await _context.Slides
                .Where(s => s.AreaId == areaId)
                .Select(s => new {
                    slideId = s.SlideId,
                    titulo = s.Titulo,
                    orden = s.Orden,
                    activo = s.Activo,
                    colorCabecera = s.ColorCabecera,
                    esGlobal = s.EsGlobal,
                    areaDestinoId = s.AreaDestinoId,
                    areaDestinoNombre = s.AreaDestinoId != null
                        ? _context.Areas.Where(a => a.AreaId == s.AreaDestinoId).Select(a => a.Nombre).FirstOrDefault()
                        : "-"
                })
                .ToListAsync();

            return Json(new { data = slides });
        }

        // CREAR SLIDE (VISTA)
        [HttpGet]
        public async Task<IActionResult> CreateSlide(int areaId)
        {
            ViewBag.AreaId = areaId;
            ViewBag.Areas = await _context.Areas
                .Where(a => a.EsInstitucional == true)
                .ToListAsync();

            return PartialView();
        }

        //FETCH CREAR SLIDE
        [HttpPost]
        public async Task<IActionResult> CreateSlideFetch([FromBody] CreateSlideDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Titulo))
                return BadRequest(new { success = false, message = "El título es obligatorio." });

            var area = await _context.Areas.FirstOrDefaultAsync(a => a.AreaId == model.AreaId);
            if (area == null)
                return BadRequest(new { success = false, message = "Área no encontrada." });

            // ✅ Validación: si es global, el área debe ser institucional
            if (model.EsGlobal)
            {
                if (!area.EsInstitucional)
                    return BadRequest(new { success = false, message = "Solo las áreas institucionales pueden tener slides globales." });

                bool existeInstitucional = await _context.Slides
                    .AnyAsync(s => s.AreaId == model.AreaId && s.EsGlobal);

                if (existeInstitucional)
                    return BadRequest(new { success = false, message = "Ya existe un slide institucional en esta área." });
            }

            var slide = new Slide
            {
                AreaId = model.AreaId,
                Titulo = model.Titulo,
                Orden = model.Orden,
                Activo = model.Activo,
                EsGlobal = model.EsGlobal,
                ColorCabecera = model.ColorCabecera,
                AreaDestinoId = model.EsGlobal ? model.AreaDestinoId : null
            };

            _context.Slides.Add(slide);
            await _context.SaveChangesAsync();

            if (!model.EsGlobal)
            {
                // ✅ Slide normal → posición fija
                var layout = new SlideLayout
                {
                    SlideId = slide.SlideId,
                    AreaId = slide.AreaId,
                    X = 0,
                    Y = 10,
                    Width = 2,
                    Height = 2
                };

                _context.SlideLayouts.Add(layout);
                await _context.SaveChangesAsync();
            }
            else
            {
                // ✅ Slide institucional → buscar espacio libre
                var (x, y) = BuscarEspacioLibre(model.AreaId, 0, 0, 2, 2);

                var layout = new SlideLayout
                {
                    SlideId = slide.SlideId,
                    AreaId = slide.AreaId,
                    X = x,
                    Y = y,
                    Width = 2,
                    Height = 2
                };

                _context.SlideLayouts.Add(layout);
                await _context.SaveChangesAsync();
            }

            var areaDestinoNombre = slide.AreaDestinoId.HasValue
                ? (await _context.Areas.FindAsync(slide.AreaDestinoId))?.Nombre ?? "-"
                : "-";

            return Ok(new
            {
                success = true,
                message = "Slide creado correctamente.",
                slideId = slide.SlideId,
                titulo = slide.Titulo,
                orden = slide.Orden,
                activo = slide.Activo,
                colorCabecera = slide.ColorCabecera,
                esGlobal = slide.EsGlobal,
                areaDestinoId = slide.AreaDestinoId,
                areaDestinoNombre = areaDestinoNombre
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditSlide([FromBody] EditSlideDto model)
        {
            var slide = await _context.Slides
                .Include(s => s.Area)
                .FirstOrDefaultAsync(x => x.SlideId == model.SlideId);

            if (slide == null)
                return BadRequest(new { success = false, message = "Slide no encontrado." });

            // ✅ Validación: si EsGlobal está marcado, debe tener un área destino
            if (model.EsGlobal && (model.AreaDestinoId == null || model.AreaDestinoId == 0))
                return BadRequest(new { success = false, message = "Debe seleccionar un área destino cuando el slide es institucional." });

            // ✅ Validación: solo puede existir un institucional por área
            if (model.EsGlobal)
            {
                bool existeOtroInstitucional = await _context.Slides
                    .AnyAsync(s => s.AreaId == slide.AreaId && s.EsGlobal && s.SlideId != slide.SlideId);

                if (existeOtroInstitucional)
                    return BadRequest(new { success = false, message = "Ya existe un slide institucional en esta área." });
            }

            // ✅ Nueva validación: si el área es institucional, debe existir al menos un slide global
            if (!model.EsGlobal && slide.Area.EsInstitucional)
            {
                bool esElUnicoGlobal = !await _context.Slides
                    .AnyAsync(s => s.AreaId == slide.AreaId && s.EsGlobal && s.SlideId != slide.SlideId);

                if (esElUnicoGlobal)
                    return BadRequest(new { success = false, message = "El área es institucional, debe existir al menos un slide global." });
            }

            // ✅ Actualizar datos
            slide.EsGlobal = model.EsGlobal;
            slide.Titulo = model.Titulo;
            slide.Orden = model.Orden;
            slide.Activo = model.Activo;
            slide.ColorCabecera = model.ColorCabecera;
            slide.AreaDestinoId = model.EsGlobal ? model.AreaDestinoId : null;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Slide actualizado correctamente." });
        }

        //FETCH ELIMINAR SLIDE
        [HttpPost]
        public async Task<IActionResult> DeleteSlide(int slideId)
        {
            var slide = await _context.Slides.FindAsync(slideId);

            if (slide == null)
                return BadRequest(new { success = false, message = "Slide no encontrado." });

            // 👇 Bloqueo: si es institucional/global no se puede borrar
            if (slide.EsGlobal)
                return BadRequest(new { success = false, message = "Este slide es institucional y no puede eliminarse." });

            _context.Slides.Remove(slide);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Slide eliminado correctamente." });
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

            var slidesGlobales = _context.Slides
                .Where(s => s.EsGlobal)
                .Include(s => s.SlideImages)
                .ToList();

            ViewBag.SlidesGlobales = slidesGlobales;

            var colores = _safetyService.ObtenerColoresDelMes(DateTime.Now.Year, DateTime.Now.Month);

            var model = new FullscreenViewModel
            {
                Area = area,
                Colores = colores
            };

            return View(model);
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
                return Ok(new { success = false, message = "Slide no encontrado." });

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

            return Ok(new { success = true, message = "Estado actualizado correctamente." });
        }
        public class ToggleSlideActivoDto
        {
            public int SlideId { get; set; }
            public bool Activo { get; set; }
        }


    }
}