using Microsoft.AspNetCore.Mvc;
using RHAds.Data;
using RHAds.Models.Safety;
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
        public IActionResult SafetyBoard()
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;

            var colores = ObtenerColoresDelMes(year, month);

            return View(colores);
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
                .OrderByDescending(e => e.Fecha)
                .ToList();

            return View(eventos);
        }

        // Modal para crear evento
        public IActionResult CrearEventoModal()
        {
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

            for (int dia = 1; dia <= 31; dia++)
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
                    colores[dia] = dia < hoy ? "green" : "";
                }
            }

            return colores;
        }
    }
}