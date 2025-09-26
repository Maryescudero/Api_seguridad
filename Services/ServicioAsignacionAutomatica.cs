using Api_seguridad.Models;
using Api_seguridad.Repositorios;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api_seguridad.Services
{
    public class ServicioAsignacionAutomatica
    {
        private readonly DataContext _db;
        private readonly RepositorioAsignacionServicio _repo;

        public ServicioAsignacionAutomatica(DataContext db, RepositorioAsignacionServicio repo)
        {
            _db = db;
            _repo = repo;
        }

        // verifica si el guardia tiene franco en x fecha
        private bool TieneFranco(Guardia g, DateOnly fecha)
        {
            return _db.FrancoGuardias.Any(f =>
                f.idGuardia == g.idGuardia &&
                f.fechaFranco == fecha);
        }

        // francos automatico rotativo
        private void GenerarFrancos(DateOnly desde, DateOnly hasta)
        {
            var guardiasFijos = _db.Guardias
                .Where(g => g.tipoGuardia == "Fijo")
                .OrderBy(g => g.idGuardia)
                .ToList();

            var francos = new List<FrancoGuardia>();
            int semana = 0;

            for (var fecha = desde; fecha <= hasta; fecha = fecha.AddDays(7))
            {
                int diaOffset = semana;
                foreach (var g in guardiasFijos)
                {
                    var fechaFranco = fecha.AddDays(diaOffset % 7);

                    if (fechaFranco >= desde && fechaFranco <= hasta)
                    {
                        bool trabajaEseDia = _db.AsignacionServicios
                            .Any(a => a.fechaAsignacion == fechaFranco && a.idGuardia == g.idGuardia && a.estado == true);

                        if (!trabajaEseDia)
                        {
                            francos.Add(new FrancoGuardia
                            {
                                idGuardia = g.idGuardia,
                                fechaFranco = fechaFranco,
                                tipoFranco = "Descanso"
                            });
                        }
                    }

                    diaOffset++;
                }
                semana++;
            }

            _db.FrancoGuardias.AddRange(francos);
            _db.SaveChanges();
        }

        // valido disponibilidad de un guardia
        private bool PuedeTrabajarHoy(List<AsignacionServicio> asignaciones, int idGuardia, DateOnly fecha, int turnoActual, HashSet<int> asignadosHoy)
        {
            if (asignadosHoy.Contains(idGuardia))
                return false;

            var turnoAyer = asignaciones
                .FirstOrDefault(a => a.idGuardia == idGuardia && a.fechaAsignacion == fecha.AddDays(-1) && a.estado == true);

            if (turnoAyer != null)
            {
                if ((turnoAyer.idTurno == 2 || turnoAyer.idTurno == 3) && turnoActual == 1)
                    return false;
            }

            return true;
        }

        // se generan asignacioneS auto  cola circular
        private void GenerarAsignaciones(DateOnly desde, DateOnly hasta)
        {
            var guardiasFijos = _db.Guardias
                .Where(g => g.tipoGuardia == "Fijo" && g.estado)
                .OrderBy(g => g.idGuardia)
                .ToList();

            if (!guardiasFijos.Any())
                throw new Exception("No se encontró ningún guardia fijo.");

            var turnosDia = _db.TurnoServicios
                .OrderBy(ts => ts.idTurno)
                .ThenBy(ts => ts.idServicio)
                .ToList();

            if (!turnosDia.Any())
                throw new Exception(" No hay configuraciones en la tabla turno_servicio.");

            var asignaciones = new List<AsignacionServicio>();
            var posPorTurno = new Dictionary<int, int>();
            int totalGuardias = guardiasFijos.Count;

            for (var fecha = desde; fecha <= hasta; fecha = fecha.AddDays(1))
            {
                var asignadosHoy = new HashSet<int>();

                foreach (var ts in turnosDia)
                {
                    if (!posPorTurno.ContainsKey(ts.idTurno))
                        posPorTurno[ts.idTurno] = 0;

                    for (int i = 0; i < ts.cantidadGuardias; i++)
                    {
                        int intentos = 0;
                        bool asignado = false;

                        while (intentos < totalGuardias && !asignado)
                        {
                            int pos = posPorTurno[ts.idTurno];
                            var g = guardiasFijos[pos];
                            posPorTurno[ts.idTurno] = (pos + 1) % totalGuardias;

                            //  Valida no repetir en el mismo dia y respetar descanso 
                            if (!asignadosHoy.Contains(g.idGuardia) &&
                                PuedeTrabajarHoy(asignaciones, g.idGuardia, fecha, ts.idTurno, asignadosHoy))
                            {
                                asignaciones.Add(new AsignacionServicio
                                {
                                    idGuardia = g.idGuardia,
                                    idServicio = ts.idServicio,
                                    idTurno = ts.idTurno,
                                    fechaAsignacion = fecha,
                                    estado = true,
                                });

                                asignadosHoy.Add(g.idGuardia);
                                asignado = true;
                            }

                            intentos++;
                        }
                    }
                }
            }

            _db.AsignacionServicios.AddRange(asignaciones);
            _db.SaveChanges();
        }


        // genera descansos extra
        private void GenerarDescansosLibres(DateOnly desde, DateOnly hasta)
        {
            var guardiasFijos = _db.Guardias.Where(g => g.tipoGuardia == "Fijo").ToList();
            var descansos = new List<FrancoGuardia>();

            for (var fecha = desde; fecha <= hasta; fecha = fecha.AddDays(1))
            {
                foreach (var g in guardiasFijos)
                {
                    bool trabajaEseDia = _db.AsignacionServicios.Any(a => a.fechaAsignacion == fecha && a.idGuardia == g.idGuardia && a.estado == true);
                    bool yaTieneFranco = _db.FrancoGuardias.Any(f => f.fechaFranco == fecha && f.idGuardia == g.idGuardia);

                    if (!trabajaEseDia && !yaTieneFranco)
                    {
                        descansos.Add(new FrancoGuardia
                        {
                            idGuardia = g.idGuardia,
                            fechaFranco = fecha,
                            tipoFranco = "Descanso"
                        });
                    }
                }
            }

            _db.FrancoGuardias.AddRange(descansos);
            _db.SaveChanges();
        }

        // genero en un solo paso
        public void GenerarAsignacionesConFrancos(DateOnly desde, DateOnly hasta)
        {
            bool existen = _db.AsignacionServicios.Any(a => a.fechaAsignacion >= desde &&
                                                            a.fechaAsignacion <= hasta &&
                                                            a.estado);
            if (existen)
                throw new Exception($"Ya existen asignaciones activas entre {desde} y {hasta}. No se puede volver a generar.");

            GenerarFrancos(desde, hasta);
            GenerarAsignaciones(desde, hasta);
            GenerarDescansosLibres(desde, hasta);
        }


        //valido si un guardia puede cubrir un servicio(cuando hay un ausente)
        public bool PuedeCubrirTurno(int idGuardia, DateOnly fecha, int idTurno)
        {
            var turnos = _db.Turnos.ToDictionary(t => t.idTurno);
            if (!turnos.ContainsKey(idTurno)) return false;

            var turnoNuevo = turnos[idTurno];
            var inicioNuevo = fecha.ToDateTime(turnoNuevo.horaInicio);
            var finNuevo = (turnoNuevo.horaFin > turnoNuevo.horaInicio)
                ? fecha.ToDateTime(turnoNuevo.horaFin)
                : fecha.AddDays(1).ToDateTime(turnoNuevo.horaFin);

            var asignaciones = _db.AsignacionServicios
                .Where(a => a.idGuardia == idGuardia &&
                            (a.fechaAsignacion == fecha.AddDays(-1) ||
                             a.fechaAsignacion == fecha ||
                             a.fechaAsignacion == fecha.AddDays(1)) &&
                             a.estado)
                .ToList();

            foreach (var a in asignaciones)
            {
                if (!turnos.ContainsKey(a.idTurno)) continue;
                var turnoAsignado = turnos[a.idTurno];

                var inicioA = a.fechaAsignacion.ToDateTime(turnoAsignado.horaInicio);
                var finA = (turnoAsignado.horaFin > turnoAsignado.horaInicio)
                    ? a.fechaAsignacion.ToDateTime(turnoAsignado.horaFin)
                    : a.fechaAsignacion.AddDays(1).ToDateTime(turnoAsignado.horaFin);

                // Si se solapa, no puede
                if (inicioA < finNuevo && inicioNuevo < finA)
                    return false;

                // calculo descanso en horas
                var descansoAntes = (inicioNuevo - finA).TotalHours;
                var descansoDespues = (inicioA - finNuevo).TotalHours;

                // REGLA: permitir si tiene entre 10 y 12h descanso, además de los >= 12h
                if ((descansoAntes >= 0 && descansoAntes < 10) ||
                    (descansoDespues >= 0 && descansoDespues < 10))
                    return false;
            }

            return true;
        }


        // asigno manual en caso de ausencia de un guardia
        public bool AsignarCobertura(int idGuardia, int idServicio, int idTurno, DateOnly fecha)
        {
            if (!PuedeCubrirTurno(idGuardia, fecha, idTurno))
                return false;
            var asignacionInactiva = _db.AsignacionServicios
                .FirstOrDefault(a =>
                    a.idServicio == idServicio &&
                    a.idTurno == idTurno &&
                    a.fechaAsignacion == fecha &&
                    a.estado == false);

            if (asignacionInactiva != null)
            {
                asignacionInactiva.idGuardia = idGuardia;
                asignacionInactiva.estado = true;
                _db.SaveChanges();
                return true;
            }

            bool yaCubierto = _db.AsignacionServicios.Any(a =>
                a.idServicio == idServicio &&
                a.idTurno == idTurno &&
                a.fechaAsignacion == fecha &&
                a.estado == true);

            if (yaCubierto) return false;
            _db.AsignacionServicios.Add(new AsignacionServicio
            {
                idGuardia = idGuardia,
                idServicio = idServicio,
                idTurno = idTurno,
                fechaAsignacion = fecha,
                estado = true,
            });

            _db.SaveChanges();
            return true;
        }

        public object AsignarCoberturaAuto(int idServicio, int idTurno, DateOnly fecha)
        {
            var asignacionInactiva = _db.AsignacionServicios
                .FirstOrDefault(a =>
                    a.idServicio == idServicio &&
                    a.idTurno == idTurno &&
                    a.fechaAsignacion == fecha &&
                    a.estado == false);

            //  primero intento cubrir con un franquero
            if (asignacionInactiva != null)
            {
                var franqueros = _db.Guardias
                    .AsNoTracking()
                    .Where(g => g.tipoGuardia == "Franquero" && g.estado)
                    .ToList();

                foreach (var f in franqueros)
                {
                    bool esAusente = _db.AsignacionServicios.Any(a =>
                        a.idGuardia == f.idGuardia &&
                        a.fechaAsignacion == fecha &&
                        a.estado == false);

                    if (!esAusente && PuedeCubrirTurno(f.idGuardia, fecha, idTurno))
                    {
                        asignacionInactiva.idGuardia = f.idGuardia;
                        asignacionInactiva.estado = true;
                        _db.SaveChanges();
                        return new
                        {
                            ok = true,
                            idGuardia = f.idGuardia,
                            nombre = f.nombre,
                            apellido = f.apellido,
                            cubiertoPor = f.nombre + " " + f.apellido
                        };
                    }
                }
            }

            // si el turno ya está cubiertosalimos
            bool yaCubierto = _db.AsignacionServicios.Any(a =>
                a.idServicio == idServicio &&
                a.idTurno == idTurno &&
                a.fechaAsignacion == fecha &&
                a.estado == true);

            if (yaCubierto)
                return new { ok = false, message = "Ese turno ya está cubierto." };

            // si no hay franqueros traigo los fijos en descanso
            var fijosDisponibles = ObtenerFijosDisponibles(idServicio, idTurno, fecha)
                .Select(f => new { f.idGuardia, f.nombre, f.apellido })
                .ToList();

            if (fijosDisponibles.Any())
            {
                return new
                {
                    ok = false,
                    message = "No hay franqueros disponibles, pero sí hay fijos en descanso.",
                    fijosDisponibles
                };
            }

            // si no hay absolutamente nadie
            return new { ok = false, message = "No hay guardias disponibles." };
        }

        public List<Guardia> ObtenerFijosDisponibles(int idServicio, int idTurno, DateOnly fecha)
        {
            var fijos = _db.Guardias
                .AsNoTracking()
                .Where(g => g.tipoGuardia == "Fijo" && g.estado)
                .ToList();

            var disponibles = new List<Guardia>();

            foreach (var g in fijos)
            {
                bool ausente = _db.AsignacionServicios
                    .Any(a => a.idGuardia == g.idGuardia &&
                              a.fechaAsignacion == fecha &&
                              a.estado == false);

                bool yaAsignado = _db.AsignacionServicios
                    .Any(a => a.idGuardia == g.idGuardia &&
                              a.fechaAsignacion == fecha &&
                              a.estado == true);

                // descartamos si ya está ausente o asignado
                if (ausente || yaAsignado) continue;

                //  usamos la nueva validacin con descanso minimo de 10h
                if (PuedeCubrirTurno(g.idGuardia, fecha, idTurno))
                {
                    disponibles.Add(g);
                }
            }

            return disponibles;
        }


    }
}
