using Api_seguridad.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Api_seguridad.Dtos; 

namespace Api_seguridad.Repositorios
{
    public class RepositorioServicio
    {
        private readonly DataContext _contexto;
        private readonly ILogger<RepositorioServicio> _logger;

        public RepositorioServicio(DataContext contexto, ILogger<RepositorioServicio> logger)
        {
            _contexto = contexto;
            _logger = logger;
        }

        // ======================= CREAR o REACTIVAR =======================
       public (bool ok, bool reactivado, int idServicio) CrearOReactivarConTurnos(ServicioDto dto)
{
    using var tx = _contexto.Database.BeginTransaction();
    try
    {
        var existente = _contexto.Servicios
            .FirstOrDefault(s => s.lugar == dto.lugar && s.direccion == dto.direccion);

        if (existente != null)
        {
            if (existente.estado)
            {
                // Ya existe activo
                return (false, false, existente.idServicio);
            }
            else
            {
                // Reactivar (NO cambiamos la fechaAlta)
                existente.estado = true;
                _contexto.Update(existente);
                _contexto.SaveChanges();
                tx.Commit();
                return (true, true, existente.idServicio);
            }
        }

        // Crear nuevo servicio
        var servicio = new Servicio
        {
            lugar = dto.lugar,
            direccion = dto.direccion,
            fechaAlta = DateOnly.FromDateTime(DateTime.Now), // fecha de alta automática
            estado = dto.estado
        };
        _contexto.Servicios.Add(servicio);
        _contexto.SaveChanges();

        if (dto.turnos?.Any() == true)
        {
            var asignaciones = dto.turnos.Select(t => new TurnoServicio
            {
                idServicio = servicio.idServicio,
                idTurno = t.idTurno,
                cantidadGuardias = t.cantidadGuardias
            });

            _contexto.TurnoServicios.AddRange(asignaciones);
            _contexto.SaveChanges();
        }

        tx.Commit();
        return (true, false, servicio.idServicio);
    }
    catch (Exception ex)
    {
        tx.Rollback();
        _logger.LogError(ex, "Error al crear o reactivar servicio con turnos");
        return (false, false, 0);
    }
}


        // ======================= ACTUALIZAR =======================
        public bool ActualizarConTurnos(int id, ServicioDto dto)
        {
            using var tx = _contexto.Database.BeginTransaction();
            try
            {
                var existente = _contexto.Servicios.FirstOrDefault(s => s.idServicio == id);
                if (existente == null) return false;

                existente.lugar = dto.lugar;
                existente.direccion = dto.direccion;
                existente.estado = dto.estado;
                // OJO: NO modificamos fechaAlta en actualización
                _contexto.Update(existente);
                _contexto.SaveChanges();


                tx.Commit();
                return true;
            }
            catch (Exception ex)
            {
                tx.Rollback();
                _logger.LogError(ex, "Error al actualizar servicio con turnos");
                return false;
            }
        }

        // ======================= OBTENER =======================
        public ServicioRespuestaDto? BuscarPorId(int id)
        {
            var servicio = _contexto.Servicios
                .Include(s => s.TurnoServicios)
                .ThenInclude(ts => ts.Turno)
                .FirstOrDefault(s => s.idServicio == id);

            if (servicio == null) return null;

            return new ServicioRespuestaDto
            {
                idServicio = servicio.idServicio,
                lugar = servicio.lugar,
                direccion = servicio.direccion,
                fechaAlta = servicio.fechaAlta,
                estado = servicio.estado,
                turnos = servicio.TurnoServicios.Select(ts => new TurnoDto
                {
                    idTurno = ts.Turno.idTurno,
                    nombre = ts.Turno.nombre,
                    hora_inicio = ts.Turno.horaInicio.ToString("HH:mm"),
                    hora_fin = ts.Turno.horaFin.ToString("HH:mm")
                }).ToList()
            };
        }


        public List<ServicioRespuestaDto> ObtenerTodos()
        {
            return _contexto.Servicios
                .Include(s => s.TurnoServicios)
                .ThenInclude(ts => ts.Turno)
                .OrderBy(s => s.idServicio)
                .Select(s => new ServicioRespuestaDto
                {
                    idServicio = s.idServicio,
                    lugar = s.lugar,
                    direccion = s.direccion,
                    fechaAlta = s.fechaAlta,
                    estado = s.estado,
                    turnos = s.TurnoServicios.Select(ts => new TurnoDto
                    {
                        idTurno = ts.Turno.idTurno,
                        nombre = ts.Turno.nombre,
                        hora_inicio = ts.Turno.horaInicio.ToString("HH:mm"),
                        hora_fin = ts.Turno.horaFin.ToString("HH:mm")
                    }).ToList()
                }).ToList();
        }

        public List<ServicioRespuestaDto> ObtenerTodosActivos()
        {
            return _contexto.Servicios
                .Where(s => s.estado) 
                .Include(s => s.TurnoServicios)
                .ThenInclude(ts => ts.Turno)
                .OrderBy(s => s.idServicio)
                .Select(s => new ServicioRespuestaDto
                {
                    idServicio = s.idServicio,
                    lugar = s.lugar,
                    direccion = s.direccion,
                    fechaAlta = s.fechaAlta,
                    estado = s.estado,
                    turnos = s.TurnoServicios.Select(ts => new TurnoDto
                    {
                        idTurno = ts.Turno.idTurno,
                        nombre = ts.Turno.nombre,
                        hora_inicio = ts.Turno.horaInicio.ToString("HH:mm"),
                        hora_fin = ts.Turno.horaFin.ToString("HH:mm")
                    }).ToList()
                })
                .ToList();
        }



        // ======================= ELIMINADO LÓGICO =======================
        public bool EliminadoLogico(int id)
        {
            try
            {
                var servicio = _contexto.Servicios.FirstOrDefault(s => s.idServicio == id);
                if (servicio == null) return false;

                servicio.estado = false;
                _contexto.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar lógicamente servicio");
                return false;
            }
        }

        // ======================= AUX: Reasignar Turnos =======================
        private void ReasignarTurnos(int idServicio, IEnumerable<int>? nuevosTurnos)
        {
            var actuales = _contexto.TurnoServicios.Where(ts => ts.idServicio == idServicio).ToList();
            if (actuales.Any())
            {
                _contexto.TurnoServicios.RemoveRange(actuales);
                _contexto.SaveChanges();
            }

            if (nuevosTurnos?.Any() == true)
            {
                var nuevos = nuevosTurnos.Select(idTurno => new TurnoServicio
                {
                    idServicio = idServicio,
                    idTurno = idTurno
                });
                _contexto.TurnoServicios.AddRange(nuevos);
                _contexto.SaveChanges();
            }
        }

      
        /// Obtiene los servicios activos, ordenados por lugar y dirección.
        
        public List<Servicio> ObtenerActivosOrdenados()
        {
            return _contexto.Servicios
                .Where(s => s.estado)
                .OrderBy(s => s.lugar)
                .ThenBy(s => s.direccion)
                .ToList();
        }
        

       
        /// Obtiene los turnos asociados a un servicio por su id.

        public List<Turno> ObtenerTurnosPorServicio(int idServicio)
        {
            return _contexto.TurnoServicios
                .Where(ts => ts.idServicio == idServicio)
                .Include(ts => ts.Turno)
                .Select(ts => ts.Turno)
                .ToList();
        }
    }
}
