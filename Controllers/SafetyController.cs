using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RHAds.Data;
using RHAds.Models; // Asegúrate de tener tu modelo aquí
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

        // Vista principal del Safety Board
        public IActionResult SafetyBoard(int? year, int? month)
        {
            int y = year ?? DateTime.Now.Year;
            int m = month ?? DateTime.Now.Month;

            var colores = ObtenerColoresDelMes(y, m);

            ViewBag.Year = y;
            ViewBag.Month = m;

            return View(colores); // ← Vista SafetyBoard.cshtml
        }

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
                    // Día con evento → color del evento
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
                    // Día sin evento
                    if (dia < hoy)
                    {
                        colores[dia] = "green"; // día pasado sin evento
                    }
                    else
                    {
                        colores[dia] = ""; // día futuro sin evento
                    }
                }
            }

            return colores;
        }
    }
}