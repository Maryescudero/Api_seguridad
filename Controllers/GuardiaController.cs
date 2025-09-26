using Microsoft.AspNetCore.Mvc;
using Api_seguridad.Models;
using Api_seguridad.Repositorios;

namespace Api_seguridad.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GuardiaController : ControllerBase
    {
        private readonly RepositorioGuardia _repositorio;

        public GuardiaController(RepositorioGuardia repositorio)
        {
            _repositorio = repositorio;
        }

        // http://localhost:5000/api/guardia   //me trae todos
        [HttpGet]
        public ActionResult<List<Guardia>> Get()
        {
            var guardias = _repositorio.ObtenerTodos();
            return Ok(guardias);
        }

        // http://localhost:5000/api/guardia/1
        [HttpGet("{id}")]
        public ActionResult<Guardia> Get(int id)
        {
            var guardia = _repositorio.BuscarPorId(id);
            if (guardia == null)
                return NotFound();
            return Ok(guardia);
        }

        //http://localhost:5000/api/guardia
        [HttpPost]
        public ActionResult<Guardia> Post([FromBody] Guardia guardia)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool exito = _repositorio.Crear(guardia);
            if (!exito)
                return StatusCode(500, "Error al crear o reactivar el guardia.");

            return Ok(guardia);
        }

        // http://localhost:5000/api/guardia/1
        [HttpPut("{id}")]
        public ActionResult<Guardia> Put(int id, [FromBody] Guardia guardia)
        {
            var existente = _repositorio.BuscarPorId(id);
            if (existente == null)
                return NotFound();

            // Actualiza todos los campos
            existente.nombre = guardia.nombre;
            existente.apellido = guardia.apellido;
            existente.documento = guardia.documento;
            existente.direccion = guardia.direccion;
            existente.telefono = guardia.telefono;
            existente.alta = guardia.alta;
            existente.estado = guardia.estado;

            bool exito = _repositorio.Actualizar(existente);
            if (!exito)
                return StatusCode(500, "Error al actualizar el guardia.");

            return Ok(existente);
        }

        // http://localhost:5000/api/guardia/(id)
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var guardia = _repositorio.BuscarPorId(id);
            if (guardia == null)
                return NotFound();

            bool exito = _repositorio.EliminadoLogico(id);
            if (!exito)
                return StatusCode(500, "Error al eliminar lógicamente el guardia.");

            return NoContent();
        }

        // http://localhost:5000/api/guardia/activos
        [HttpGet("activos")]
        public ActionResult<List<Guardia>> GetActivos()
        {
            var activos = _repositorio.ObtenerActivos();
            return Ok(activos);
        }

        // http://localhost:5000/api/guardia/documento/30784123
        [HttpGet("documento/{documento}")]
        public ActionResult<Guardia> GetPorDocumento(string documento)
        {
            var guardia = _repositorio.BuscarPorDocumento(documento);
            if (guardia == null)
                return NotFound();

            return Ok(guardia);
        }
    }
}
