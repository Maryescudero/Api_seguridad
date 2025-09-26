using Api_seguridad.Dtos;
using Api_seguridad.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api_seguridad.Repositorios
{
    public class RepositorioAsignacionServicio
    {
        private readonly DataContext _contexto;
        private readonly ILogger<RepositorioAsignacionServicio> _logger;

        public RepositorioAsignacionServicio(DataContext ctx, ILogger<RepositorioAsignacionServicio> logger)
        {
            _contexto = ctx;
            _logger = logger;
        }

        // ===================== CRUD =====================
        public bool Crear(AsignacionServicio a)
        {
            try
            {
                _contexto.AsignacionServicios.Add(a);
                return _contexto.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear AsignacionServicio");
                return false;
            }
        }

        public bool Actualizar(AsignacionServicio a)
        {
            try
            {
                var exis = _contexto.AsignacionServicios.FirstOrDefault(x => x.idAsignacionServicio == a.idAsignacionServicio);
                if (exis == null) return false;

                exis.idGuardia = a.idGuardia;
                exis.idServicio = a.idServicio;
                exis.idTurno = a.idTurno;
                exis.fechaAsignacion = a.fechaAsignacion;

                return _contexto.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar AsignacionServicio");
                return false;
            }
        }

        public AsignacionServicio BuscarPorId(int id)
            => _contexto.AsignacionServicios.AsNoTracking().FirstOrDefault(x => x.idAsignacionServicio == id);

        public List<AsignacionServicio> ObtenerTodos()
            => _contexto.AsignacionServicios.AsNoTracking().OrderBy(x => x.fechaAsignacion).ToList();

        public bool Eliminar(int id)
        {
            var exis = _contexto.AsignacionServicios.FirstOrDefault(x => x.idAsignacionServicio == id);
            if (exis == null) return false;
            _contexto.AsignacionServicios.Remove(exis);
            return _contexto.SaveChanges() > 0;
        }

        // ===================== Consultas / Helpers =====================

        public bool GuardiaTieneAsignacionElDia(int idGuardia, DateOnly fecha)
        {
            return _contexto.AsignacionServicios.Any(a =>
                a.idGuardia == idGuardia &&
                a.fechaAsignacion == fecha);
        }

        public bool YaAsignadoMismoServicioTurnoDia(int idGuardia, int idServicio, int idTurno, DateOnly fecha)
        {
            return _contexto.AsignacionServicios.Any(a =>
                a.idGuardia == idGuardia &&
                a.idServicio == idServicio &&
                a.idTurno == idTurno &&
                a.fechaAsignacion == fecha);
        }

        public int CargaEnPeriodo(int idGuardia, DateOnly ini, DateOnly fin)
        {
            return _contexto.AsignacionServicios.Count(a =>
                a.idGuardia == idGuardia &&
                a.fechaAsignacion >= ini &&
                a.fechaAsignacion <= fin);
        }

        public Turno? ObtenerTurnoPorId(int idTurno)
        {
            try
            {
                return _contexto.Turnos
                    .AsNoTracking()
                    .FirstOrDefault(t => t.idTurno == idTurno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener turno con id {idTurno}");
                return null;
            }
        }

        public List<AsignacionServicio> ObtenerAsignacionesPorGuardiaYPeriodo(int idGuardia, int? mes, int? anio)
        {
            var q = _contexto.AsignacionServicios.Where(a => a.idGuardia == idGuardia);
            if (mes.HasValue) q = q.Where(a => a.fechaAsignacion.Month == mes.Value);
            if (anio.HasValue) q = q.Where(a => a.fechaAsignacion.Year == anio.Value);
            return q.OrderBy(a => a.fechaAsignacion).ToList();
        }
        public List<AsignacionServicio> ObtenerAsignacionesPorGuardia(int idGuardia)
            => _contexto.AsignacionServicios
                        .Where(a => a.idGuardia == idGuardia)
                        .OrderBy(a => a.fechaAsignacion)
                        .ToList();


        // ===================== DTOs Detallados =====================

        // Detalle por guardia y período, con posibilidad de pedir solo pendientes (no cumplidos)
        public List<AsignacionServicioDto> ObtenerDetalladasPorGuardiaYPeriodo(int idGuardia, int? mes = null, int? anio = null, bool? soloPendientes = null)
        {
            var servicios = _contexto.Servicios.AsNoTracking().ToDictionary(s => s.idServicio);
            var turnos = _contexto.Turnos.AsNoTracking().ToDictionary(t => t.idTurno);
            var guardias = _contexto.Guardias.AsNoTracking().ToDictionary(g => g.idGuardia);

            var q = _contexto.AsignacionServicios.AsNoTracking().Where(a => a.idGuardia == idGuardia);
            if (mes.HasValue) q = q.Where(a => a.fechaAsignacion.Month == mes.Value);
            if (anio.HasValue) q = q.Where(a => a.fechaAsignacion.Year == anio.Value);

            var asignaciones = q.OrderBy(a => a.fechaAsignacion).ToList();
            if (!asignaciones.Any()) return new List<AsignacionServicioDto>();

            int m = mes ?? asignaciones.First().fechaAsignacion.Month;
            int y = anio ?? asignaciones.First().fechaAsignacion.Year;

            var historiales = _contexto.HistorialUsuarios.AsNoTracking()
                .Where(h => h.fecha.Year == y && h.fecha.Month == m)
                .ToList()
                .ToDictionary(h => (h.idGuardia, h.idServicio, h.fecha), h => h);

            var salida = new List<AsignacionServicioDto>();

            foreach (var a in asignaciones)
            {
                var fecha = a.fechaAsignacion;


                servicios.TryGetValue(a.idServicio, out var s);
                turnos.TryGetValue(a.idTurno, out var t);
                guardias.TryGetValue(a.idGuardia, out var g);
                historiales.TryGetValue((a.idGuardia, a.idServicio, fecha), out var h);

                bool cumplido = h != null && h.ingreso.HasValue && h.egreso.HasValue;

                if (soloPendientes == true && cumplido) continue;
                if (soloPendientes == false && !cumplido) continue;

                salida.Add(new AsignacionServicioDto
                {
                    id_asignacionServicio = a.idAsignacionServicio,
                    id_guardia = a.idGuardia,
                    id_servicio = a.idServicio,
                    id_turno = a.idTurno,
                    fecha = fecha,
                    estado = a.estado,

                    lugar = s?.lugar ?? "",
                    direccion = s?.direccion ?? "",
                    // FIX: Servicio ya no tiene 'turno'
                    nombreTurno = t?.nombre ?? "",
                    horario = (t != null) ? $"{t.horaInicio:HH\\:mm} - {t.horaFin:HH\\:mm}" : "",

                    nombre = g?.nombre ?? "",
                    apellido = g?.apellido ?? "",
                    documento = g?.documento ?? "",

                    puntualidad = h?.puntualidad ?? "",
                    ingreso = h?.ingreso.HasValue == true ? h.ingreso.Value.ToString("HH\\:mm") : "",
                    egreso = h?.egreso.HasValue == true ? h.egreso!.Value.ToString("HH\\:mm") : "",
                    cumplido = cumplido
                });
            }

            return salida;
        }

        // Resumen mensual (detalle + totales) con Historial
        public AsignacionesResumenDto ObtenerAsignacionesPorMesYAnio(int mes, int anio)
        {
            var servicios = _contexto.Servicios.AsNoTracking().ToDictionary(s => s.idServicio);
            var turnos = _contexto.Turnos.AsNoTracking().ToDictionary(t => t.idTurno);
            var guardias = _contexto.Guardias.AsNoTracking().ToDictionary(g => g.idGuardia);

            var historiales = _contexto.HistorialUsuarios.AsNoTracking()
                .Where(h => h.fecha.Month == mes && h.fecha.Year == anio)
                .ToList()
                .ToDictionary(h => (h.idGuardia, h.idServicio, h.fecha), h => h);

            var asignaciones = _contexto.AsignacionServicios.AsNoTracking()
                .Where(a => a.fechaAsignacion.Month == mes && a.fechaAsignacion.Year == anio)
                .OrderBy(a => a.fechaAsignacion)
                .ToList();

            var salida = new List<AsignacionServicioDto>();

            foreach (var a in asignaciones)
            {
                var fecha = a.fechaAsignacion;


                servicios.TryGetValue(a.idServicio, out var s);
                turnos.TryGetValue(a.idTurno, out var t);
                guardias.TryGetValue(a.idGuardia, out var g);
                historiales.TryGetValue((a.idGuardia, a.idServicio, fecha), out var h);

                salida.Add(new AsignacionServicioDto
                {
                    id_asignacionServicio = a.idAsignacionServicio,
                    id_guardia = a.idGuardia,
                    id_servicio = a.idServicio,
                    id_turno = a.idTurno,
                    fecha = fecha,
                    estado = a.estado,

                    lugar = s?.lugar ?? "",
                    direccion = s?.direccion ?? "",
                    // FIX: Servicio ya no tiene 'turno'
                    nombreTurno = t?.nombre ?? "",
                    horario = (t != null) ? $"{t.horaInicio:HH\\:mm} - {t.horaFin:HH\\:mm}" : "",

                    nombre = g?.nombre ?? "",
                    apellido = g?.apellido ?? "",
                    documento = g?.documento ?? "",

                    puntualidad = h?.puntualidad ?? "",
                    ingreso = h?.ingreso.HasValue == true ? h.ingreso.Value.ToString("HH\\:mm") : "",
                    egreso = h?.egreso.HasValue == true ? h.egreso!.Value.ToString("HH\\:mm") : "",
                    cumplido = h != null && h.ingreso.HasValue && h.egreso.HasValue
                });
            }

            return new AsignacionesResumenDto
            {
                asignaciones = salida,
                totalAsignaciones = salida.Count,
                guardiasUtilizados = salida.Select(x => x.id_guardia).Distinct().Count(),
                serviciosCubiertos = salida.Select(x => x.id_servicio).Distinct().Count()
            };
        }


        // Buscar por documento, nombre o apellido + fecha exacta (día, mes, año)
        public List<AsignacionServicioDto> BuscarAsignacionesPorCriterio(string criterio, int dia, int mes, int anio)
        {
            var servicios = _contexto.Servicios.AsNoTracking().ToDictionary(s => s.idServicio);
            var turnos = _contexto.Turnos.AsNoTracking().ToDictionary(t => t.idTurno);
            var guardias = _contexto.Guardias.AsNoTracking().ToDictionary(g => g.idGuardia);

            var historiales = _contexto.HistorialUsuarios.AsNoTracking()
                .Where(h => h.fecha.Day == dia && h.fecha.Month == mes && h.fecha.Year == anio)
                .ToList()
                .ToDictionary(h => (h.idGuardia, h.idServicio, h.fecha), h => h);

            //  Buscar coincidencias por documento, nombre o apellido
            var guardiasIds = _contexto.Guardias
                .Where(g =>
                    g.documento == criterio ||
                    g.nombre.Contains(criterio) ||
                    g.apellido.Contains(criterio))
                .Select(g => g.idGuardia)
                .ToList();

            if (!guardiasIds.Any()) return new List<AsignacionServicioDto>();

            var asignaciones = _contexto.AsignacionServicios.AsNoTracking()
                .Where(a => guardiasIds.Contains(a.idGuardia) &&
                            a.fechaAsignacion.Day == dia &&
                            a.fechaAsignacion.Month == mes &&
                            a.fechaAsignacion.Year == anio)
                .OrderBy(a => a.fechaAsignacion)
                .ToList();

            var salida = new List<AsignacionServicioDto>();
            foreach (var a in asignaciones)
            {
                var fecha = a.fechaAsignacion;

                servicios.TryGetValue(a.idServicio, out var s);
                turnos.TryGetValue(a.idTurno, out var t);
                guardias.TryGetValue(a.idGuardia, out var g);
                historiales.TryGetValue((a.idGuardia, a.idServicio, fecha), out var h);

                salida.Add(new AsignacionServicioDto
                {
                    id_asignacionServicio = a.idAsignacionServicio,
                    id_guardia = a.idGuardia,
                    id_servicio = a.idServicio,
                    id_turno = a.idTurno,
                    fecha = fecha,
                    estado = a.estado,

                    lugar = s?.lugar ?? "",
                    direccion = s?.direccion ?? "",
                    nombreTurno = t?.nombre ?? "",
                    horario = (t != null) ? $"{t.horaInicio:HH\\:mm} - {t.horaFin:HH\\:mm}" : "",

                    nombre = g?.nombre ?? "",
                    apellido = g?.apellido ?? "",
                    documento = g?.documento ?? "",

                    puntualidad = h?.puntualidad ?? "",
                    ingreso = h?.ingreso.HasValue == true ? h.ingreso.Value.ToString("HH\\:mm") : "",
                    egreso = h?.egreso.HasValue == true ? h.egreso!.Value.ToString("HH\\:mm") : "",
                    cumplido = h != null && h.ingreso.HasValue && h.egreso.HasValue
                });
            }

            return salida;
        }
public List<AsignacionServicioDto> BuscarAsignacionesPorLugarYFecha(string lugar, DateOnly fecha)
{
    // 1. Obtenemos los ids de servicios que matchean el lugar
    var serviciosIds = _contexto.Servicios
        .Where(s => s.lugar.Contains(lugar))
        .Select(s => s.idServicio)
        .ToList();

    // 2. Traemos diccionarios auxiliares
    var servicios = _contexto.Servicios.AsNoTracking().ToDictionary(s => s.idServicio);
    var turnos = _contexto.Turnos.AsNoTracking().ToDictionary(t => t.idTurno);
    var guardias = _contexto.Guardias.AsNoTracking().ToDictionary(g => g.idGuardia);

    var historiales = _contexto.HistorialUsuarios.AsNoTracking()
        .Where(h => h.fecha == fecha)
        .ToList()
        .ToDictionary(h => (h.idGuardia, h.idServicio, h.fecha), h => h);

    // 3. Ahora sí, filtramos asignaciones
    var asignaciones = _contexto.AsignacionServicios.AsNoTracking()
        .Where(a => a.fechaAsignacion == fecha && serviciosIds.Contains(a.idServicio))
        .OrderBy(a => a.fechaAsignacion)
        .ToList();

    // 4. Construimos la salida DTO
    var salida = new List<AsignacionServicioDto>();
    foreach (var a in asignaciones)
    {
        servicios.TryGetValue(a.idServicio, out var s);
        turnos.TryGetValue(a.idTurno, out var t);
        guardias.TryGetValue(a.idGuardia, out var g);
        historiales.TryGetValue((a.idGuardia, a.idServicio, fecha), out var h);

        salida.Add(new AsignacionServicioDto
        {
            id_asignacionServicio = a.idAsignacionServicio,
            id_guardia = a.idGuardia,
            id_servicio = a.idServicio,
            id_turno = a.idTurno,
            fecha = fecha,
            lugar = s?.lugar ?? "",
            direccion = s?.direccion ?? "",
            nombreTurno = t?.nombre ?? "",
            horario = t != null ? $"{t.horaInicio:HH\\:mm} - {t.horaFin:HH\\:mm}" : "",
            nombre = g?.nombre ?? "",
            apellido = g?.apellido ?? "",
            documento = g?.documento ?? "",
            puntualidad = h?.puntualidad ?? "",
            ingreso = h?.ingreso.HasValue == true ? h.ingreso.Value.ToString("HH:mm") : "",
            egreso = h?.egreso.HasValue == true ? h.egreso.Value.ToString("HH:mm") : "",
            cumplido = h != null && h.ingreso.HasValue && h.egreso.HasValue
        });
    }

    return salida;
}


        // Listar todas las asignaciones detalladas por mes/año
        public List<AsignacionServicioDto> ObtenerDetalladasPorMesYAnio(int mes, int anio)
        {
            var servicios = _contexto.Servicios.AsNoTracking().ToDictionary(s => s.idServicio);
            var turnos = _contexto.Turnos.AsNoTracking().ToDictionary(t => t.idTurno);
            var guardias = _contexto.Guardias.AsNoTracking().ToDictionary(g => g.idGuardia);

            var historiales = _contexto.HistorialUsuarios.AsNoTracking()
                .Where(h => h.fecha.Month == mes && h.fecha.Year == anio)
                .ToList()
                .ToDictionary(h => (h.idGuardia, h.idServicio, h.fecha), h => h);

            var asignaciones = _contexto.AsignacionServicios.AsNoTracking()
                .Where(a => a.fechaAsignacion.Month == mes && a.fechaAsignacion.Year == anio)
                .OrderBy(a => a.fechaAsignacion)
                .ToList();

            var salida = new List<AsignacionServicioDto>();
            foreach (var a in asignaciones)
            {
                var fecha = a.fechaAsignacion;

                servicios.TryGetValue(a.idServicio, out var s);
                turnos.TryGetValue(a.idTurno, out var t);
                guardias.TryGetValue(a.idGuardia, out var g);
                historiales.TryGetValue((a.idGuardia, a.idServicio, fecha), out var h);

                salida.Add(new AsignacionServicioDto
                {
                    id_asignacionServicio = a.idAsignacionServicio,
                    id_guardia = a.idGuardia,
                    id_servicio = a.idServicio,
                    id_turno = a.idTurno,
                    fecha = fecha,
                    estado = a.estado,

                    lugar = s?.lugar ?? "",
                    direccion = s?.direccion ?? "",
                    nombreTurno = t?.nombre ?? "",
                    horario = (t != null) ? $"{t.horaInicio:HH\\:mm} - {t.horaFin:HH\\:mm}" : "",

                    nombre = g?.nombre ?? "",
                    apellido = g?.apellido ?? "",
                    documento = g?.documento ?? "",

                    puntualidad = h?.puntualidad ?? "",
                    ingreso = h?.ingreso.HasValue == true ? h.ingreso.Value.ToString("HH\\:mm") : "",
                    egreso = h?.egreso.HasValue == true ? h.egreso!.Value.ToString("HH\\:mm") : "",
                    cumplido = h != null && h.ingreso.HasValue && h.egreso.HasValue
                });
            }

            return salida;
        }

    }
}
