using Api_seguridad.Dtos;
using Api_seguridad.Repositorios;
using Api_seguridad.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_seguridad.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AsistenciaController : ControllerBase
    {
        private readonly QrTokenService _qrToken;
        private readonly RepositorioAsignacionServicio _repoAsign;
        private readonly RepositorioHistorialUsuario _repoHist;
        private readonly RepositorioUsuario _repoUsuario;
        private readonly ServicioAsignacionAutomatica _serAsign;
        private readonly RepositorioNotificacion _repoNotificacion;

        public AsistenciaController(
            QrTokenService qrToken,
            RepositorioAsignacionServicio repoAsign,
            RepositorioHistorialUsuario repoHist,
            RepositorioUsuario repoUsuario,
            ServicioAsignacionAutomatica serAsign,
            RepositorioNotificacion repoNotificacion)
        {
            _qrToken = qrToken;
            _repoAsign = repoAsign;
            _repoHist = repoHist;
            _repoUsuario = repoUsuario;
            _serAsign = serAsign;
            _repoNotificacion = repoNotificacion;
        }

        public class ScanRequest { public string token { get; set; } = null!; }

        // scan
        [HttpPost("scan")]
        [Authorize]
        public async Task<IActionResult> Scan([FromBody] ScanRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.token))
                return BadRequest("Token requerido.");

            var (ok, idServicio, idTurno, fecha, tipo, error) = _qrToken.Validar(req.token);
            if (!ok)
                return Unauthorized($"Token QR inválido/expirado: {error}");

            var idGuardiaClaim = User.FindFirst("id_guardia")?.Value;
            if (string.IsNullOrEmpty(idGuardiaClaim))
                return Unauthorized("No se pudo determinar el id_guardia.");

            int idGuardia = int.Parse(idGuardiaClaim);

            var asignacion = _repoAsign.ObtenerAsignacionesPorGuardiaYPeriodo(idGuardia, fecha.Month, fecha.Year)
                .FirstOrDefault(a =>
                    a.idServicio == idServicio &&
                    a.idTurno == idTurno &&
                    a.fechaAsignacion == fecha);

            if (asignacion == null)
                return BadRequest("No tenés asignación para este servicio/turno/fecha.");

            var ahora = DateTime.Now;
            var turno = _repoAsign.ObtenerTurnoPorId(idTurno);
            if (turno == null)
                return StatusCode(500, "El turno no existe.");

            TimeSpan horaInicio = turno.horaInicio.ToTimeSpan();
            TimeSpan tolerancia = horaInicio.Add(TimeSpan.FromMinutes(10));

            string puntualidad = (ahora.TimeOfDay > tolerancia)
                ? "tardanza"
                : "puntual";

            bool okHist = (tipo == "ingreso")
             ? _repoHist.MarcarIngreso(idGuardia, idServicio, fecha, ahora, puntualidad)
             : _repoHist.MarcarEgreso(idGuardia, idServicio, fecha, ahora);

            if (!okHist)
            {
                string detalle = (tipo == "ingreso")
                    ? "⚠️ Ya existe un registro de ingreso para este guardia en este servicio y fecha."
                    : "⚠️ No se puede registrar egreso sin un ingreso previo o ya existe un egreso registrado.";

                return BadRequest(detalle);
            }


            // 🔔 Crear notificación al/los admin(s)
            string msg = $"Guardia {idGuardia} fichó {tipo} en servicio {idServicio}, turno {idTurno} a las {ahora:HH:mm} ({puntualidad})";

            var usuarioGuardia = _repoUsuario.BuscarPorGuardia(idGuardia);
            if (usuarioGuardia == null)
                return StatusCode(500, "No se encontró un usuario asociado al guardia.");

            // obtener todos los administradores activos
            var admins = _repoUsuario.ObtenerAdministradores();
            if (!admins.Any())
                return StatusCode(500, "No se encontró ningún administrador activo.");

            foreach (var admin in admins)
            {
                await _repoNotificacion.CrearNotificacion(
                    msg,
                    usuarioGuardia.idUsuario, // enviado por guardia
                    admin.idUsuario,          // recibido por admin
                    "admin"
                );
            }

            return Ok(new
            {
                ok = true,
                accion = tipo,
                servicio = idServicio,
                turno = idTurno,
                fecha,
                hora = ahora.ToString("HH:mm"),
                puntualidad
            });
        }

        // presentes
        [HttpGet("presentes")]
        [Authorize(Roles = "administrador")]
        public IActionResult GetPresentes()
        {
            var presentes = _repoHist.ObtenerPresentes();
            var dto = _repoHist.MapearADto(presentes);
            return Ok(dto);
        }

        // reporte mensual
        //  /api/asistencia/reporte/mensual/6?mes=9&anio=2025
        [HttpGet("reporte/mensual/{idGuardia}")]
        [Authorize(Roles = "administrador")]
        public IActionResult ReporteMensual(int idGuardia, [FromQuery] int mes, [FromQuery] int anio)
        {
            var reporte = _repoHist.ObtenerReporteMensual(idGuardia, mes, anio);
            return Ok(reporte);
        }

        // reporte x servicio y fecha
        //  /api/asistencia/reporte/servicio/5?fecha=2025-09-03
        [HttpGet("reporte/servicio/{idServicio}")]
        [Authorize(Roles = "administrador")]
        public IActionResult GetReporteServicio(int idServicio, [FromQuery] DateOnly fecha)
        {
            var reporte = _repoHist.ObtenerAsistenciasPorServicioYFecha(idServicio, fecha);
            return Ok(reporte);
        }

        //  Resumen mensual totsal
        //  /api/asistencia/resumen/mensual/6?mes=9&anio=2025
        [HttpGet("resumen/mensual/{idGuardia}")]
        
        public IActionResult ResumenMensual(int idGuardia, [FromQuery] int mes, [FromQuery] int anio)
        {
            var resumen = _repoHist.ObtenerResumenMensual(idGuardia, mes, anio);
            return Ok(resumen);
        }

        // reporte consolidado
        //  /api/asistencia/reporte/mensual/consolidado?mes=9&anio=2025
        [HttpGet("reporte/mensual/consolidado")]
        [Authorize(Roles = "administrador")]
        public IActionResult ReporteMensualConsolidado([FromQuery] int mes, [FromQuery] int anio)
        {
            var reporte = _repoHist.ObtenerReporteMensualConsolidado(mes, anio);
            return Ok(reporte);
        }

        //ausente y asignacion

        // http://localhost:5000/api/asistencia/registrar-ausente?idGuardia=6&idServicio=5&fecha=2025-09-09
        [HttpPost("registrar-ausente")]
        [Authorize(Roles = "administrador")]
        public IActionResult RegistrarAusente(
    [FromQuery] int idGuardia,
    [FromQuery] int idServicio,
    [FromQuery] DateOnly fecha,
    [FromQuery] string? observaciones)
        {
            var ok = _repoHist.RegistrarAusente(idGuardia, idServicio, fecha, observaciones);
            if (!ok)
            {
                return StatusCode(500, new AsignacionResponseDto
                {
                    Ok = false,
                    Error = "No se pudo registrar el ausente o ya estaba marcado."
                });
            }

            return Ok(new AsignacionResponseDto
            {
                Ok = true,
                Message = "Ausencia registrada y asignación desactivada.",
                Data = new { idGuardia, idServicio, fecha, tipo = "ausente", observaciones }
            });
        }



        // http://localhost:5000/api/asistencia/asignar-cobertura?idGuardia=17&idServicio=5&idTurno=2&fecha=2025-09-09
        [HttpPost("asignar-cobertura")]
        [Authorize(Roles = "administrador")]
        public IActionResult AsignarCobertura(int idGuardia, int idServicio, int idTurno, DateOnly fecha)
        {
            var ok = _serAsign.AsignarCobertura(idGuardia, idServicio, idTurno, fecha);

            if (!ok)
            {
                return BadRequest(new AsignacionResponseDto
                {
                    Ok = false,
                    Error = "El guardia no puede cubrir este turno (solapamiento, más de 12h, ya está asignado o asignación activa aún existe)."
                });
            }

            return Ok(new AsignacionResponseDto
            {
                Ok = true,
                Message = "Cobertura asignada correctamente.",
                Data = new { idGuardia, idServicio, idTurno, fecha }
            });
        }


        // http://localhost:5000/api/asistencia/asignar-cobertura-auto?idServicio=5&idTurno=2&fecha=2025-09-09
        [HttpPost("asignar-cobertura-auto")]
        [Authorize(Roles = "administrador")]
        public IActionResult AsignarCoberturaAuto(int idServicio, int idTurno, DateOnly fecha)
        {
            var result = _serAsign.AsignarCoberturaAuto(idServicio, idTurno, fecha);

            if ((bool)result.GetType().GetProperty("ok")!.GetValue(result)!)
            {
                return Ok(new AsignacionResponseDto
                {
                    Ok = true,
                    Message = "Cobertura asignada automáticamente.",
                    Data = result
                });
            }
            return Ok(new AsignacionResponseDto
            {
                Ok = false,
                Error = "No hay guardias disponibles",
                Data = result
            });
        }
        // reporte diario
// /api/asistencia/reporte/diario?fecha=2025-09-20
[HttpGet("reporte/diario")]
[Authorize(Roles = "administrador")]
public IActionResult ReporteDiario([FromQuery] DateOnly fecha)
{
    var reporte = _repoHist.ObtenerReporteDiario(fecha);
    return Ok(reporte);
}



    }
}