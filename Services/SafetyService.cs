using System;
using System.Collections.Generic;
using System.Linq;
using RHAds.Data;
using RHAds.Models.Safety; // enum TipoEvento
using RHAds.Models;        // entidad SafetyEvent

namespace RHAds.Services
{
    public class SafetyService
    {
        private readonly AppDbContext _context;

        public SafetyService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Genera el diccionario de colores para la Safety Cross
        /// usando clases Bootstrap según los eventos registrados en el mes.
        /// </summary>
        public Dictionary<int, string> ObtenerColoresDelMes(int year, int month)
        {
            var eventos = _context.SafetyEvents
                .Where(e => e.Fecha.Year == year && e.Fecha.Month == month)
                .ToList();

            var colores = new Dictionary<int, string>();
            int hoy = DateTime.Now.Day;
            int diasEnMes = DateTime.DaysInMonth(year, month);

            for (int dia = 1; dia <= 31; dia++)
            {
                if (dia <= diasEnMes)
                {
                    var evento = eventos.FirstOrDefault(e => e.Fecha.Day == dia);

                    if (evento != null)
                    {
                        colores[dia] = evento.Tipo switch
                        {
                            TipoEvento.Accidente => "bg-danger",   // rojo
                            TipoEvento.Incidente => "bg-warning",  // amarillo
                            TipoEvento.NearMiss => "bg-primary",  // azul
                            _ => ""
                        };
                    }
                    else
                    {
                        // Día válido pero sin evento → verde si ya pasó
                        colores[dia] = dia < hoy ? "bg-success" : "";
                    }
                }
                else
                {
                    // Día inexistente en el mes → gris
                    colores[dia] = "bg-secondary";
                }
            }

            return colores;
        }
    }
}