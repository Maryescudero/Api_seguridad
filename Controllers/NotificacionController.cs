using Microsoft.AspNetCore.Mvc;
using Api_seguridad.Repositorios;
using Api_seguridad.Dtos;

namespace Api_seguridad.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificacionController : ControllerBase
    {
        private readonly RepositorioNotificacion _repoNotificacion;

        public NotificacionController(RepositorioNotificacion repoNotificacion)
        {
            _repoNotificacion = repoNotificacion;
        }

        // POST api/notificacion/enviar
        [HttpPost("enviar")]
        public async Task<IActionResult> Enviar([FromBody] NotificacionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Mensaje))
                return BadRequest(new { ok = false, mensaje = "El mensaje no puede estar vacío." });

            var dto = await _repoNotificacion.CrearNotificacion(
                request.Mensaje,
                request.EnviadaPor,
                request.EnviadaA,
                request.RolDestino
            );

            return Ok(new { ok = true, mensaje = "Notificación enviada.", data = dto });
        }

        // POST api/notificacion/enviar-quincena
        [HttpPost("enviar-quincena")]
        public async Task<IActionResult> EnviarQuincena([FromBody] QuincenaRequest request)
        {
            await _repoNotificacion.EnviarAvisoQuincena(request.enviadaPor);

            return Ok(new { ok = true, mensaje = "Aviso de quincena enviado." });
        }


        // GET api/notificacion/mis/3
        [HttpGet("mis/{idUsuario}")]
        public IActionResult MisNotificaciones(int idUsuario)
        {
            var notifs = _repoNotificacion.ObtenerPorUsuario(idUsuario);

            var response = notifs.Select(n => new NotificacionResponse
            {
                IdNotificacion = n.id_notificacion,
                Mensaje = n.mensaje,
                Fecha = n.fecha_envio.ToString("yyyy-MM-dd HH:mm"),
                EnviadaPor = n.enviada_por,
                EnviadaA = n.enviada_a,
                Leido = n.leido
            }).ToList();

            return Ok(new
            {
                ok = true,
                total = response.Count,
                data = response
            });
        }

        // PUT api/notificacion/marcar-leida/20?id_usuario=4
        [HttpPut("marcar-leida/{id_notificacion}")]
        public async Task<IActionResult> MarcarLeida(
            [FromRoute] int id_notificacion,
            [FromQuery(Name = "id_usuario")] int id_usuario)
        {
            var dto = await _repoNotificacion.MarcarComoLeida(id_notificacion, id_usuario);

            if (dto == null)
                return NotFound(new { ok = false, mensaje = "Notificación no encontrada." });

            return Ok(new { ok = true, mensaje = "Notificación marcada como leída.", data = dto });
        }
    }
}
