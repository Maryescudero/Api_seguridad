using Api_seguridad.Services; 
using Api_seguridad.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Api_seguridad.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QrController : ControllerBase
    {
        private readonly QrTokenService _tokenSvc;
        private readonly QrCodeService _qrSvc;
        private readonly RepositorioTurnoServicio _repoTurnoServicio;

        public QrController(
            QrTokenService tokenSvc,
            QrCodeService qrSvc,
            RepositorioTurnoServicio repoTurnoServicio)
        {
            _tokenSvc = tokenSvc;
            _qrSvc = qrSvc;
            _repoTurnoServicio = repoTurnoServicio;
        }

        //  Endpoint original: devuelve PNG
        [HttpGet("servicio/{idServicio}/turno/{idTurno}")]
        public IActionResult Get(int idServicio, int idTurno, [FromQuery] DateOnly? fecha = null, [FromQuery] string tipo = "ingreso")
        {
            if (tipo != "ingreso" && tipo != "egreso") 
                return BadRequest("tipo inválido");

            var turnoValido = _repoTurnoServicio.ExisteTurnoParaServicio(idServicio, idTurno);
            if (!turnoValido)
                return BadRequest("El turno no está asignado a este servicio.");

            var f = fecha ?? DateOnly.FromDateTime(DateTime.Today);

            var token = _tokenSvc.GenerarTokenQr(idServicio, idTurno, f, tipo);
            var png = _qrSvc.GenerarPng(token);

            return File(png, "image/png");
        }

        //  NUEVO endpoint: devuelve el token en JSON
        [HttpGet("servicio/{idServicio}/turno/{idTurno}/token")]
        public IActionResult GetToken(int idServicio, int idTurno, [FromQuery] DateOnly? fecha = null, [FromQuery] string tipo = "ingreso")
        {
            if (tipo != "ingreso" && tipo != "egreso") 
                return BadRequest("tipo inválido");

            var turnoValido = _repoTurnoServicio.ExisteTurnoParaServicio(idServicio, idTurno);
            if (!turnoValido)
                return BadRequest("El turno no está asignado a este servicio.");

            var f = fecha ?? DateOnly.FromDateTime(DateTime.Today);

            var token = _tokenSvc.GenerarTokenQr(idServicio, idTurno, f, tipo);

            return Ok(new
            {
                servicio = idServicio,
                turno = idTurno,
                fecha = f,
                tipo,
                token
            });
        }
    }
}
