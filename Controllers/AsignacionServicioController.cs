using Api_seguridad.Dtos;
using Api_seguridad.Models;
using Api_seguridad.Repositorios;
using Api_seguridad.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api_seguridad.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AsignacionServicioController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly RepositorioAsignacionServicio _repo;
        private readonly ServicioAsignacionAutomatica _service;

        public AsignacionServicioController(
            DataContext context,
            RepositorioAsignacionServicio repo,
            ServicioAsignacionAutomatica service)
        {
            _context = context;
            _repo = repo;
            _service = service;
        }

        // ===== CRUD =====

        [HttpGet]
        public ActionResult<List<AsignacionServicio>> Get()
            => Ok(_repo.ObtenerTodos());

        [HttpGet("{id}")]
        public ActionResult<AsignacionServicio> Get(int id)
        {
            var a = _repo.BuscarPorId(id);
            if (a == null) return NotFound();
            return Ok(a);
        }

        [HttpPost]
        public ActionResult Post([FromBody] AsignacionServicio a)
        {
            var ok = _repo.Crear(a);
            if (!ok) return StatusCode(500, "No se pudo crear la asignación");
            return Ok(a);
        }

        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] AsignacionServicio a)
        {
            if (id != a.idAsignacionServicio) return BadRequest("ID no coincide");
            var ok = _repo.Actualizar(a);
            if (!ok) return StatusCode(500, "No se pudo actualizar");
            return Ok(a);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var ok = _repo.Eliminar(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        // ===== PARA USO DE GUARDIA =====

        // Todas las asignaciones crudas del guardia (sin detalle)
        [HttpGet("guardia/{id_guardia}")]
        public ActionResult<List<AsignacionServicio>> GetPorGuardia(int id_guardia)
            => Ok(_repo.ObtenerAsignacionesPorGuardia(id_guardia));



        //http://localhost:5000/api/asignacionservicio/guardia/6/detalle?mes=9&anio=2025
        [HttpGet("guardia/{id_guardia}/detalle")]
        public ActionResult<List<AsignacionServicioDto>> GetDetallePorGuardia(int id_guardia, int? mes = null, int? anio = null)
            => Ok(_repo.ObtenerDetalladasPorGuardiaYPeriodo(id_guardia, mes, anio, null));


        // PENDIENTES (no cumplidas) por guardia con detalle. Filtros opcionales mes/año
        [HttpGet("guardia/{id_guardia}/pendientes")]
        public ActionResult<List<AsignacionServicioDto>> GetPendientesPorGuardia(int id_guardia, int? mes = null, int? anio = null)
            => Ok(_repo.ObtenerDetalladasPorGuardiaYPeriodo(id_guardia, mes, anio, true));


        // =====PARA USO ADMIN =====

        // Buscar por documento/nombre/apellido + fecha exacta
        [HttpGet("buscar")]
        public ActionResult<List<AsignacionServicioDto>> BuscarAsignaciones(
    [FromQuery] string criterio, [FromQuery] int dia, [FromQuery] int mes, [FromQuery] int anio)
    => Ok(_repo.BuscarAsignacionesPorCriterio(criterio, dia, mes, anio));


        // Resumen mensual (detalle + totales + historial)
        [HttpGet("listar-por-periodo")]
        public ActionResult<AsignacionesResumenDto> ListarPorPeriodo([FromQuery] int mes, [FromQuery] int anio)
            => Ok(_repo.ObtenerAsignacionesPorMesYAnio(mes, anio));


        // Listar todas las asignaciones detalladas por periodo
        [HttpGet("listar-asignaciones-por-periodo")]
        public ActionResult<List<AsignacionServicioDto>> ListarAsignacionesPorPeriodo(
            [FromQuery] int mes, [FromQuery] int anio)
            => Ok(_repo.ObtenerDetalladasPorMesYAnio(mes, anio));


        // Buscar por lugar y fecha
        [HttpGet("buscar-por-lugar")]
        public ActionResult<List<AsignacionServicioDto>> BuscarPorLugarYFecha(
            [FromQuery] string lugar,
            [FromQuery] DateOnly fecha)
        {
            return Ok(_repo.BuscarAsignacionesPorLugarYFecha(lugar, fecha));
        }


        // ===== GENERAR QUINCENA  =====

        // POST: /api/asignacionservicio/generar-quincena-con-francos?fechaInicio=2025-09-01
        [HttpPost("generar-quincena-con-francos")]
        public ActionResult<object> GenerarQuincenaConFrancos([FromQuery] DateOnly fechaInicio)
        {
            var fechaFin = fechaInicio.AddDays(14);

            _service.GenerarAsignacionesConFrancos(fechaInicio, fechaFin);

            //  Contar asignaciones generadas en ese rango
            int totalGeneradas = _context.AsignacionServicios
                .Count(a => a.fechaAsignacion >= fechaInicio
                         && a.fechaAsignacion <= fechaFin
                         && a.estado);

            return Ok(new
            {
                Mensaje = "Asignaciones + francos generados correctamente",
                Desde = fechaInicio.ToString("yyyy-MM-dd"),
                Hasta = fechaFin.ToString("yyyy-MM-dd"),
                TotalGeneradas = totalGeneradas
            });
        }




        // ===== FRANCOS =====
        // GET: /api/asignacionservicio/francos/guardia/5?desde=2025-08-18&hasta=2025-09-01
        [HttpGet("francos/guardia/{idGuardia}")]
        public ActionResult<List<FrancoDto>> GetFrancosGuardia(
    int idGuardia,
    [FromQuery] DateTime desde,
    [FromQuery] DateTime hasta)
        {
            var ini = DateOnly.FromDateTime(desde);
            var fin = DateOnly.FromDateTime(hasta);

            var salida = (from f in _context.FrancoGuardias
                          join g in _context.Guardias on f.idGuardia equals g.idGuardia
                          where f.idGuardia == idGuardia
                                && f.fechaFranco >= ini
                                && f.fechaFranco <= fin
                          orderby f.fechaFranco
                          select new FrancoDto
                          {
                              idFranco = f.idFranco,
                              idGuardia = f.idGuardia,
                              nombre = g.nombre,
                              apellido = g.apellido,
                              documento = g.documento,
                              fechaFranco = f.fechaFranco.ToString("yyyy-MM-dd"),
                              tipoFranco = f.tipoFranco
                          }).ToList();

            if (!salida.Any())
                return NotFound($"No hay francos entre {ini} y {fin} para el guardia {idGuardia}");

            return Ok(salida);
        }


        // GET: /api/asignacionservicio/francos?desde=2025-09-16&hasta=2025-09-30
        [HttpGet("francos")]
        public ActionResult<List<FrancoDto>> GetFrancosPorFechas(
            [FromQuery] DateTime desde,
            [FromQuery] DateTime hasta)
        {
            var ini = DateOnly.FromDateTime(desde);
            var fin = DateOnly.FromDateTime(hasta);

            var salida = (from f in _context.FrancoGuardias
                          join g in _context.Guardias on f.idGuardia equals g.idGuardia
                          where f.fechaFranco >= ini && f.fechaFranco <= fin
                          orderby f.fechaFranco, g.apellido, g.nombre
                          select new FrancoDto
                          {
                              idFranco = f.idFranco,
                              idGuardia = f.idGuardia,
                              nombre = g.nombre,
                              apellido = g.apellido,
                              documento = g.documento,
                              fechaFranco = f.fechaFranco.ToString("yyyy-MM-dd"),
                              tipoFranco = f.tipoFranco
                          }).ToList();

            if (!salida.Any())
                return NotFound($"No hay francos entre {ini} y {fin}");

            return Ok(salida);
        }


        // GET: /api/asignacionservicio/francos/documento/30784123?desde=2025-09-16&hasta=2025-09-30
        [HttpGet("francos/documento/{documento}")]
        public ActionResult<List<FrancoDto>> GetFrancosPorDocumento(
            string documento,
            [FromQuery] DateTime desde,
            [FromQuery] DateTime hasta)
        {
            var ini = DateOnly.FromDateTime(desde);
            var fin = DateOnly.FromDateTime(hasta);

            var salida = (from f in _context.FrancoGuardias
                          join g in _context.Guardias on f.idGuardia equals g.idGuardia
                          where g.documento == documento
                                && f.fechaFranco >= ini
                                && f.fechaFranco <= fin
                          orderby f.fechaFranco
                          select new FrancoDto
                          {
                              idFranco = f.idFranco,
                              idGuardia = f.idGuardia,
                              nombre = g.nombre,
                              apellido = g.apellido,
                              documento = g.documento,
                              fechaFranco = f.fechaFranco.ToString("yyyy-MM-dd"),
                              tipoFranco = f.tipoFranco
                          }).ToList();

            if (!salida.Any())
                return NotFound($"No hay francos entre {ini} y {fin} para el documento {documento}");

            return Ok(salida);
        }


    }
}
