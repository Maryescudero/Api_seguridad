using Api_seguridad.Repositorios;
using Api_seguridad.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Api_seguridad.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicioController : ControllerBase
    {
        private readonly RepositorioServicio _repo;
        private readonly ILogger<ServicioController> _logger;

        public ServicioController(RepositorioServicio repo, ILogger<ServicioController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        // ======================= GET: Todos =======================
        //http://localhost:5000/api/servicio
        [HttpGet]
        public ActionResult<List<ServicioRespuestaDto>> GetTodos()
        {
            var servicios = _repo.ObtenerTodos();
            return Ok(servicios);
        }

        // ======================= GET: activos =======================
        //http://localhost:5000/api/servicio/activos
        [HttpGet("activos")]
        public ActionResult<List<ServicioRespuestaDto>> GetActivos()
        {
            var servicios = _repo.ObtenerTodosActivos();
            return Ok(servicios);
        }


        // ======================= GET: Por Id =======================
        //http://localhost:5000/api/servicio(id)
        [HttpGet("{id}")]
        public ActionResult<ServicioRespuestaDto> GetPorId(int id)
        {
            var servicio = _repo.BuscarPorId(id);
            if (servicio == null)
                return NotFound($"No se encontró servicio con id {id}");
            return Ok(servicio);
        }

        // ======================= POST: Crear =======================
        //http://localhost:5000/api/servicio
        [HttpPost]
        public IActionResult Crear([FromBody] ServicioDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (ok, reactivado, idServicio) = _repo.CrearOReactivarConTurnos(dto);

            if (!ok && !reactivado)
                return Conflict("Ya existe un servicio activo con ese lugar y dirección.");
            if (!ok)
                return StatusCode(500, "Error al crear o reactivar el servicio.");

            if (reactivado)
                return Ok(new { mensaje = "Servicio reactivado correctamente", idServicio });

            return Ok(new { mensaje = "Servicio creado correctamente", idServicio });
        }

        // ======================= PUT: Actualizar =======================
        //http://localhost:5000/api/servicio(id)
        [HttpPut("{id}")]
        public IActionResult Actualizar(int id, [FromBody] ServicioDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var actualizado = _repo.ActualizarConTurnos(id, dto);
            if (!actualizado)
                return NotFound($"No se encontró servicio con id {id}");

            return Ok(new { mensaje = "Servicio actualizado correctamente", id });
        }

        // ======================= DELETE: Eliminado lógico =======================
        //http://localhost:5000/api/servicio(id)
        [HttpDelete("{id}")]
        public IActionResult EliminarLogico(int id)
        {
            var eliminado = _repo.EliminadoLogico(id);
            if (!eliminado)
                return NotFound($"No se encontró servicio con id {id}");

            return NoContent();
        }
    }
}
