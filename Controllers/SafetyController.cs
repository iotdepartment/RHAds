using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RHAds.Data;
using RHAds.Models.Areas;
using RHAds.Models.Safety;
using RHAds.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RHAds.Controllers
{
    public class SafetyController : Controller
    {
        private readonly AppDbContext _context;

        public SafetyController(AppDbContext context)
        {
            _context = context;
        }

        // ================================
        //  SAFETY BOARD (solo visualización)
        // ================================
        private SafetyEvent? ObtenerUltimoEvento(TipoEvento tipo)
        {
            return _context.SafetyEvents
                .Include(e => e.Area) // <-- importante
                .Where(e => e.Tipo == tipo)
                .OrderByDescending(e => e.Fecha)
                .FirstOrDefault();
        }

        public IActionResult SafetyBoard()
        {
            var colores = ObtenerColoresDelMes(DateTime.Now.Year, DateTime.Now.Month);

            var model = new SafetyBoardViewModel
            {
                Colores = colores,
                DiasDesdeIncidente = DiasDesdeUltimoEvento(TipoEvento.Incidente),
                DiasDesdeAccidente = DiasDesdeUltimoEvento(TipoEvento.Accidente),
                DiasDesdeNearMiss = DiasDesdeUltimoEvento(TipoEvento.NearMiss),
                UltimoIncidente = ObtenerUltimoEvento(TipoEvento.Incidente),
                UltimoAccidente = ObtenerUltimoEvento(TipoEvento.Accidente),
                UltimoNearMiss = ObtenerUltimoEvento(TipoEvento.NearMiss)
            };

            return View(model);
        }

        public IActionResult SafetyBoardFull()
        {
            var colores = ObtenerColoresDelMes(DateTime.Now.Year, DateTime.Now.Month);

            var model = new SafetyBoardViewModel
            {
                Colores = colores,
                DiasDesdeIncidente = DiasDesdeUltimoEvento(TipoEvento.Incidente),
                DiasDesdeAccidente = DiasDesdeUltimoEvento(TipoEvento.Accidente),
                DiasDesdeNearMiss = DiasDesdeUltimoEvento(TipoEvento.NearMiss),
                UltimoIncidente = ObtenerUltimoEvento(TipoEvento.Incidente),
                UltimoAccidente = ObtenerUltimoEvento(TipoEvento.Accidente),
                UltimoNearMiss = ObtenerUltimoEvento(TipoEvento.NearMiss)
            };

            // Renderiza la vista con el layout especial
            return View("SafetyBoardFull", model);
        }

        public IActionResult SafetyBoardPartial()
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;

            var colores = ObtenerColoresDelMes(year, month);

            return PartialView("Partials/_SafetyBoardPartial", colores);
        }

        // ================================
        //  INCIDENTS (lista + crear eventos)
        // ================================
        public IActionResult Incidents()
        {
            var eventos = _context.SafetyEvents
                .Include(e => e.Area)
                .OrderByDescending(e => e.Fecha)
                .ToList();

            return View(eventos);
        }

        private int DiasDesdeUltimoEvento(TipoEvento tipo)
        {
            var ultimo = _context.SafetyEvents
                .Where(e => e.Tipo == tipo)
                .OrderByDescending(e => e.Fecha)
                .Select(e => e.Fecha)
                .FirstOrDefault();

            return ultimo == default ? 0 : (DateTime.Today - ultimo).Days;
        }
            
        // Modal para crear evento
        public IActionResult CrearEventoModal()
        {
            var areas = _context.Areas
                .Where(a => a.Activo)
                .OrderBy(a => a.Nombre)
                .ToList();

            ViewBag.Areas = areas;

            return PartialView("Partials/_CrearEventoModal", new SafetyEvent());
        }

        // Guardar evento vía AJAX
        [HttpPost]
        public IActionResult CrearEventoAjax([FromBody] SafetyEvent model)
        {
            if (model == null)
                return Json(new { success = false, message = "Datos inválidos" });

            _context.SafetyEvents.Add(model);
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                id = model.Id,
                areaNombre = _context.Areas.Find(model.AreaId)?.Nombre,
                tipoBadge = model.Tipo switch
                {
                    TipoEvento.Accidente => "<span class='badge bg-danger rounded-pill'>Accidente</span>",
                    TipoEvento.Incidente => "<span class='badge bg-warning text-dark rounded-pill'>Incidente</span>",
                    TipoEvento.NearMiss => "<span class='badge bg-primary rounded-pill'>Near Miss</span>",
                    _ => "<span class='badge bg-secondary rounded-pill'>Desconocido</span>"
                }
            });
        }

        // MODAL EDITAR
        public IActionResult EditarEventoModal(int id)
        {
            var evento = _context.SafetyEvents.Find(id);
            ViewBag.Areas = _context.Areas.Where(a => a.Activo).ToList();
            return PartialView("Partials/_EditarEventoModal", evento);
        }

        // AJAX EDITAR
        [HttpPost]
        public IActionResult EditarEventoAjax([FromBody] SafetyEvent model)
        {
            _context.SafetyEvents.Update(model);
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                areaNombre = _context.Areas.Find(model.AreaId)?.Nombre,
                tipoBadge = model.Tipo switch
                {
                    TipoEvento.Accidente => "<span class='badge bg-danger rounded-pill'>Accidente</span>",
                    TipoEvento.Incidente => "<span class='badge bg-warning text-dark rounded-pill'>Incidente</span>",
                    TipoEvento.NearMiss => "<span class='badge bg-primary rounded-pill'>Near Miss</span>",
                    _ => "<span class='badge bg-secondary rounded-pill'>Desconocido</span>"
                }
            });
        }

        // AJAX ELIMINAR
        [HttpDelete]
        public IActionResult EliminarEventoAjax(int id)
        {
            var evento = _context.SafetyEvents.Find(id);
            _context.SafetyEvents.Remove(evento);
            _context.SaveChanges();
            return Json(new { success = true });
        }


        // ================================
        //  LÓGICA DE COLORES
        // ================================
        private Dictionary<int, string> ObtenerColoresDelMes(int year, int month)
        {
            var eventos = _context.SafetyEvents
                .Where(e => e.Fecha.Year == year && e.Fecha.Month == month)
                .ToList();

            var colores = new Dictionary<int, string>();
            int hoy = DateTime.Now.Day;
            int diasEnMes = DateTime.DaysInMonth(year, month); // número real de días

            for (int dia = 1; dia <= 31; dia++)
            {
                if (dia <= diasEnMes)
                {
                    var evento = eventos.FirstOrDefault(e => e.Fecha.Day == dia);

                    if (evento != null)
                    {
                        colores[dia] = evento.Tipo switch
                        {
                            TipoEvento.Accidente => "red",
                            TipoEvento.Incidente => "yellow",
                            TipoEvento.NearMiss => "blue",
                            _ => ""
                        };
                    }
                    else
                    {
                        // Día válido pero sin evento → verde si ya pasó
                        colores[dia] = dia < hoy ? "green" : "";
                    }
                }
                else
                {
                    // Día inexistente en el mes → color neutro
                    colores[dia] = "gray";
                }
            }

            return colores;
        }
    }
}