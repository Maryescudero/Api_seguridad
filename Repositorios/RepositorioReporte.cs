using Api_seguridad.DTOs;
using Api_seguridad.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api_seguridad.Repositorios
{
    public class RepositorioReporte
    {
        private readonly DataContext _contexto;
        private readonly ILogger<RepositorioReporte> _logger;

        public RepositorioReporte(DataContext contexto, ILogger<RepositorioReporte> logger)
        {
            _contexto = contexto;
            _logger = logger;
        }

        // ======================= CONSOLIDADO MENSUAL =======================
        public List<ResumenMensualConsolidadoDto> ObtenerResumenMensual(int mes, int anio)
        {
            try
            {
                var guardias = _contexto.Guardias.AsNoTracking().ToList();
                var resultado = new List<ResumenMensualConsolidadoDto>();

                foreach (var g in guardias)
                {
                    // 📌 Servicios asignados en el período
                    var asignaciones = _contexto.AsignacionServicios
                        .Where(a => a.idGuardia == g.idGuardia &&
                                    a.fechaAsignacion.Month == mes &&
                                    a.fechaAsignacion.Year == anio)
                        .ToList();

                    int totalAsignados = asignaciones.Count;

                    // 📌 Historial en el período
                    var historial = _contexto.HistorialUsuarios
                        .Where(h => h.idGuardia == g.idGuardia &&
                                    h.fecha.Month == mes &&
                                    h.fecha.Year == anio)
                        .ToList();

                    int totalCubiertos = historial.Count(h => h.ingreso.HasValue);

                    // 📌 Cálculo de horas
                    int horasDiurnas = 0, horasNocturnas = 0;
                    foreach (var h in historial)
                    {
                        if (h.ingreso.HasValue && h.egreso.HasValue)
                        {
                            // 🔗 Vincular historial con asignación para conocer el turno
                            var asignacion = asignaciones.FirstOrDefault(a =>
                                a.idGuardia == h.idGuardia &&
                                a.idServicio == h.idServicio &&
                                a.fechaAsignacion == h.fecha);

                            if (asignacion != null)
                            {
                                var turno = _contexto.Turnos.FirstOrDefault(t => t.idTurno == asignacion.idTurno);
                                if (turno != null)
                                {
                                    var horas = (turno.horaFin - turno.horaInicio).TotalHours;
                                    if (turno.nombre.ToLower().Contains("noche"))
                                        horasNocturnas += (int)horas;
                                    else
                                        horasDiurnas += (int)horas;
                                }
                            }
                        }
                    }

                    //  Construcción DTO (sin ausencias, sin francos)
                    resultado.Add(new ResumenMensualConsolidadoDto
                    {
                        Guardia = g.idGuardia,     // ID interno (oculto en PDF)
                        Documento = g.documento,   // Mostramos documento en el PDF
                        Nombre = g.nombre,
                        Apellido = g.apellido,
                        Mes = mes,
                        Anio = anio,
                        TotalServiciosAsignados = totalAsignados,
                        TotalServiciosCubiertos = totalCubiertos,
                        HorasDiurnas = $"{horasDiurnas}h",
                        HorasNocturnas = $"{horasNocturnas}h",
                        HorasTotales = $"{horasDiurnas + horasNocturnas}h"
                    });
                }

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reporte consolidado mensual");
                return new List<ResumenMensualConsolidadoDto>();
            }
        }
    }
}
