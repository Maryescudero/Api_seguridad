using Microsoft.AspNetCore.Mvc;
using Api_seguridad.Models;
using Api_seguridad.Repositorios;

namespace Api_seguridad.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TurnoController : ControllerBase
    {
        private readonly RepositorioTurno _repositorio;

        public TurnoController(RepositorioTurno repositorio)
        {
            _repositorio = repositorio;
        }

        // GET: api/turno
        [HttpGet]
        public ActionResult<List<Turno>> Get()
        {
            return Ok(_repositorio.ObtenerTodos());
        }

        // GET: api/turno/1
        [HttpGet("{id}")]
        public ActionResult<Turno> Get(int id)
        {
            var turno = _repositorio.BuscarPorId(id);
            if (turno == null)
                return NotFound();

            return Ok(turno);
        }

        // POST: api/turno
        [HttpPost]
        public ActionResult<Turno> Post([FromBody] Turno turno)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool exito = _repositorio.Crear(turno);
            if (!exito)
                return StatusCode(500, "Error al crear el turno.");

            return Ok(turno);
        }

        // PUT: api/turno/1
        [HttpPut("{id}")]
        public ActionResult<Turno> Put(int id, [FromBody] Turno turno)
        {
            var existente = _repositorio.BuscarPorId(id);
            if (existente == null)
                return NotFound();

            existente.nombre = turno.nombre;
            existente.horaInicio = turno.horaInicio;
            existente.horaFin = turno.horaFin;

            bool exito = _repositorio.Actualizar(existente);
            if (!exito)
                return StatusCode(500, "Error al actualizar el turno.");

            return Ok(existente);
        }

        // DELETE: api/turno/1
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var turno = _repositorio.BuscarPorId(id);
            if (turno == null)
                return NotFound();

            bool exito = _repositorio.EliminadoLogico(id);
            if (!exito)
                return StatusCode(500, "Error al eliminar el turno.");

            return NoContent();
        }
    }
}
