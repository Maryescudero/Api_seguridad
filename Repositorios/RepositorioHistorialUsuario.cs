using Api_seguridad.Dtos;
using Api_seguridad.Models;
using Microsoft.EntityFrameworkCore;

namespace Api_seguridad.Repositorios
{
    public class RepositorioHistorialUsuario
    {
        private readonly DataContext _ctx;

        public RepositorioHistorialUsuario(DataContext ctx)
        {
            _ctx = ctx;
        }

        // Marca ingreso
        public bool MarcarIngreso(int idGuardia, int idServicio, DateOnly fecha, DateTime horaIngreso, string puntualidad)
        {
            try
            {
                var registro = _ctx.HistorialUsuarios
                    .FirstOrDefault(h => h.idGuardia == idGuardia &&
                                         h.idServicio == idServicio &&
                                         h.fecha == fecha);

                if (registro == null)
                {
                    registro = new HistorialUsuario
                    {
                        idGuardia = idGuardia,
                        idServicio = idServicio,
                        fecha = fecha,
                        tipo = "asistencia",
                        ingreso = TimeOnly.FromDateTime(horaIngreso),
                        puntualidad = puntualidad
                    };
                    _ctx.HistorialUsuarios.Add(registro);
                }
                else
                {
                    if (registro.ingreso.HasValue)
                        return false;

                    registro.tipo = "asistencia";
                    registro.ingreso = TimeOnly.FromDateTime(horaIngreso);
                    registro.puntualidad = puntualidad;
                }

                return _ctx.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }


        // MARCA EGRESO
        public bool MarcarEgreso(int idGuardia, int idServicio, DateOnly fecha, DateTime ahora, string? observaciones = null)
        {
            try
            {
                var h = _ctx.HistorialUsuarios
                    .FirstOrDefault(x => x.idGuardia == idGuardia &&
                                         x.idServicio == idServicio &&
                                         x.fecha == fecha);
                if (h == null || !h.ingreso.HasValue)
                    return false;

                if (h.egreso.HasValue)
                    return false;

                // Registrar egreso válido
                h.tipo = "asistencia";
                h.egreso = TimeOnly.FromDateTime(ahora);

                var horas = CalcularHoras(h.ingreso.Value, h.egreso.Value);
                h.observaciones = FormatearHoras(horas) + " trabajados";

                return _ctx.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }



        // registrar ausente manual admin
        public bool RegistrarAusente(int idGuardia, int idServicio, DateOnly fecha, string? observaciones = null)
        {
            using var transaction = _ctx.Database.BeginTransaction();
            try
            {

                var asignacion = _ctx.AsignacionServicios
                    .FirstOrDefault(a =>
                        a.idGuardia == idGuardia &&
                        a.idServicio == idServicio &&
                        a.fechaAsignacion == fecha &&
                        a.estado);

                if (asignacion != null)
                {
                    asignacion.estado = false;
                    _ctx.AsignacionServicios.Update(asignacion);
                }
                var h = _ctx.HistorialUsuarios
                    .FirstOrDefault(x =>
                        x.idGuardia == idGuardia &&
                        x.idServicio == idServicio &&
                        x.fecha == fecha);
                if (h == null)
                {
                    h = new HistorialUsuario
                    {
                        idGuardia = idGuardia,
                        idServicio = idServicio,
                        fecha = fecha,
                        tipo = "ausente",
                        puntualidad = null,
                        ingreso = null,
                        egreso = null,
                        observaciones = string.IsNullOrWhiteSpace(observaciones)
                            ? "Ausencia sin justificar"
                            : observaciones
                    };
                    _ctx.HistorialUsuarios.Add(h);
                }
                else
                {
                    h.tipo = "ausente";
                    h.puntualidad = null;
                    h.ingreso = null;
                    h.egreso = null;
                    h.observaciones = string.IsNullOrWhiteSpace(observaciones)
                        ? h.observaciones
                        : observaciones;
                }
                _ctx.SaveChanges();
                transaction.Commit();

                return true;
            }
            catch (Exception)
            {
                transaction.Rollback();
                return false;
            }
        }

        // lista de presentes
        public List<HistorialUsuario> ObtenerPresentes()
        {
            return _ctx.HistorialUsuarios
                .Include(h => h.Guardia)
                .Include(h => h.Servicio)
                .Where(h => h.tipo == "asistencia" && h.ingreso != null && h.egreso == null)
                .ToList();
        }

        // mapeo dto
        public List<AsistenciaDto> MapearADto(List<HistorialUsuario> historial)
        {
            var servicios = _ctx.Servicios.ToDictionary(s => s.idServicio, s => s.lugar);

            return historial.Select(h => new AsistenciaDto
            {
                idHistorial = h.IdHistorial,
                idGuardia = h.idGuardia,
                nombreGuardia = h.Guardia?.nombre ?? "",
                apellidoGuardia = h.Guardia?.apellido ?? "",
                idServicio = h.idServicio,
                lugarServicio = servicios.ContainsKey(h.idServicio) ? servicios[h.idServicio] : "",
                fecha = h.fecha,
                tipo = h.tipo ?? "",
                puntualidad = h.puntualidad,
                ingreso = h.ingreso?.ToString("HH:mm"),
                egreso = h.egreso?.ToString("HH:mm"),
                observaciones = h.observaciones,
                horasTrabajadas = (h.ingreso.HasValue && h.egreso.HasValue)
                    ? CalcularHoras(h.ingreso.Value, h.egreso.Value)
                    : 0
            }).ToList();
        }

        // reporte diario
        public object ObtenerReporteDiario(DateOnly fecha)
        {
            var lista = MapearADto(
                _ctx.HistorialUsuarios
                    .Include(h => h.Guardia)
                    .Include(h => h.Servicio)
                    .Where(h => h.fecha == fecha)
                    .ToList()
            );

            var totalAsignados = _ctx.AsignacionServicios.Count(a => a.fechaAsignacion == fecha);
            var totalCubiertos = lista.Count(d => d.tipo == "asistencia");

            return new
            {
                fecha,
                totalServiciosAsignados = totalAsignados,
                totalServiciosCubiertos = totalCubiertos,
                puntuales = lista.Count(d => d.puntualidad == "puntual"),
                tardanzas = lista.Count(d => d.puntualidad == "tardanza"),
                ausencias = lista.Count(d => d.tipo == "ausente"),
                francos = lista.Count(d => d.tipo == "franco"),
                horasTotales = FormatearHoras(lista.Sum(d => d.horasTrabajadas)),
                detalle = lista
            };
        }

        // repo mensual
        public object ObtenerReporteMensual(int idGuardia, int mes, int anio)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var guardia = _ctx.Guardias.FirstOrDefault(g => g.idGuardia == idGuardia);

            var detalle = MapearADto(
                _ctx.HistorialUsuarios
                    .Where(h => h.idGuardia == idGuardia && h.fecha.Month == mes && h.fecha.Year == anio)
                    .Include(h => h.Guardia)
                    .Include(h => h.Servicio)
                    .ToList()
            );

            var totalAsignados = _ctx.AsignacionServicios.Count(a => a.idGuardia == idGuardia &&
                                                                     a.fechaAsignacion.Month == mes &&
                                                                     a.fechaAsignacion.Year == anio);
            var totalCubiertos = detalle.Count(d => d.tipo == "asistencia");

            var francosTotalesMes = _ctx.FrancoGuardias.Count(f =>
                f.idGuardia == idGuardia &&
                f.fechaFranco.Month == mes &&
                f.fechaFranco.Year == anio);

            var francosHastaHoy = _ctx.FrancoGuardias.Count(f =>
                f.idGuardia == idGuardia &&
                f.fechaFranco.Month == mes &&
                f.fechaFranco.Year == anio &&
                f.fechaFranco <= hoy);

            return new
            {
                guardia = idGuardia,
                nombre = guardia?.nombre,
                apellido = guardia?.apellido,
                mes,
                anio,
                totalServiciosAsignados = totalAsignados,
                totalServiciosCubiertos = totalCubiertos,
                puntuales = detalle.Count(d => d.puntualidad == "puntual"),
                tardanzas = detalle.Count(d => d.puntualidad == "tardanza"),
                ausencias = detalle.Count(d => d.tipo == "ausente"),
                francosTotalesMes,
                francosHastaHoy,
                horasTotales = FormatearHoras(detalle.Sum(d => d.horasTrabajadas)),
                detalle
            };
        }

        // mensual resumido
        public object ObtenerResumenMensual(int idGuardia, int mes, int anio)
        {
            var reporte = ObtenerReporteMensual(idGuardia, mes, anio) as dynamic;

            return new
            {
                guardia = reporte.guardia,
                nombre = reporte.nombre,
                apellido = reporte.apellido,
                mes = reporte.mes,
                anio = reporte.anio,
                totalServiciosAsignados = reporte.totalServiciosAsignados,
                totalServiciosCubiertos = reporte.totalServiciosCubiertos,
                puntuales = reporte.puntuales,
                tardanzas = reporte.tardanzas,
                ausencias = reporte.ausencias,
                francosTotalesMes = reporte.francosTotalesMes,
                francosHastaHoy = reporte.francosHastaHoy,
                horasTotales = reporte.horasTotales
            };
        }

        // asistencia x servicio y fecha
        public object ObtenerAsistenciasPorServicioYFecha(int idServicio, DateOnly fecha)
        {
            var lista = MapearADto(
                _ctx.HistorialUsuarios
                    .Include(h => h.Guardia)
                    .Include(h => h.Servicio)
                    .Where(h => h.idServicio == idServicio && h.fecha == fecha)
                    .ToList()
            );

            var totalAsignados = _ctx.AsignacionServicios.Count(a => a.idServicio == idServicio &&
                                                                    a.fechaAsignacion == fecha);
            var totalCubiertos = lista.Count(d => d.tipo == "asistencia");

            return new
            {
                servicio = idServicio,
                fecha,
                totalServiciosAsignados = totalAsignados,
                totalServiciosCubiertos = totalCubiertos,
                puntuales = lista.Count(d => d.puntualidad == "puntual"),
                tardanzas = lista.Count(d => d.puntualidad == "tardanza"),
                ausencias = lista.Count(d => d.tipo == "ausente"),
                francos = lista.Count(d => d.tipo == "franco"),
                horasTotales = FormatearHoras(lista.Sum(d => d.horasTrabajadas)),
                detalle = lista
            };
        }

        // calcular horas
        private double CalcularHoras(TimeOnly ingreso, TimeOnly egreso)
        {
            var inicio = ingreso.ToTimeSpan();
            var fin = egreso.ToTimeSpan();

            if (fin >= inicio)
                return (fin - inicio).TotalHours;

            return (fin + TimeSpan.FromHours(24) - inicio).TotalHours;
        }

        // metodo para devolver bien el formato de hora
        private string FormatearHoras(double totalHoras)
        {
            int horas = (int)totalHoras;
            int minutos = (int)Math.Round((totalHoras - horas) * 60);
            return $"{horas}h {minutos}m";
        }

        // reporte d todos los guardias
        public object ObtenerReporteMensualConsolidado(int mes, int anio)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);

            // Traer todos los guardias
            var guardias = _ctx.Guardias.ToList();

            var lista = new List<object>();

            foreach (var g in guardias)
            {
                var detalle = MapearADto(
                    _ctx.HistorialUsuarios
                        .Where(h => h.idGuardia == g.idGuardia && h.fecha.Month == mes && h.fecha.Year == anio)
                        .Include(h => h.Guardia)
                        .Include(h => h.Servicio)
                        .ToList()
                );

                var totalAsignados = _ctx.AsignacionServicios.Count(a => a.idGuardia == g.idGuardia &&
                                                                         a.fechaAsignacion.Month == mes &&
                                                                         a.fechaAsignacion.Year == anio);
                var totalCubiertos = detalle.Count(d => d.tipo == "asistencia");

                var francosTotalesMes = _ctx.FrancoGuardias.Count(f =>
                    f.idGuardia == g.idGuardia &&
                    f.fechaFranco.Month == mes &&
                    f.fechaFranco.Year == anio);

                var francosHastaHoy = _ctx.FrancoGuardias.Count(f =>
                    f.idGuardia == g.idGuardia &&
                    f.fechaFranco.Month == mes &&
                    f.fechaFranco.Year == anio &&
                    f.fechaFranco <= hoy);

                lista.Add(new
                {
                    guardia = g.idGuardia,
                    nombre = g.nombre,
                    apellido = g.apellido,
                    mes,
                    anio,
                    totalServiciosAsignados = totalAsignados,
                    totalServiciosCubiertos = totalCubiertos,
                    puntuales = detalle.Count(d => d.puntualidad == "puntual"),
                    tardanzas = detalle.Count(d => d.puntualidad == "tardanza"),
                    ausencias = detalle.Count(d => d.tipo == "ausente"),
                    francosTotalesMes,
                    francosHastaHoy,
                    horasTotales = FormatearHoras(detalle.Sum(d => d.horasTrabajadas))
                });
            }

            return lista;
        }
  


    }
}
