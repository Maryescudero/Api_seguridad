using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Api_seguridad.Models;
using Api_seguridad.Repositorios;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Api_seguridad.Dtos;

namespace Api_seguridad.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly RepositorioUsuario _repositorioUsuario;

        public UsuarioController(RepositorioUsuario repositorioUsuario)
        {
            _repositorioUsuario = repositorioUsuario;
        }

        [HttpGet]
        public ActionResult<List<Usuario>> Get()
        {
            return Ok(_repositorioUsuario.ObtenerTodos());
        }

        [HttpGet("{id}")]
        public ActionResult<Usuario> Get(int id)
        {
            var usuario = _repositorioUsuario.BuscarPorId(id);
            if (usuario == null) return NotFound();
            return Ok(usuario);
        }


        //http://localhost:5000/api/usuario
        [HttpPost]
        [AllowAnonymous]
        public ActionResult<Usuario> Post([FromBody] Usuario u)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (_repositorioUsuario.ObtenerPorEmail(u.email) != null)
                return BadRequest("El correo ya está en uso.");

            bool exito = _repositorioUsuario.Crear(u);
            if (!exito) return StatusCode(500, "Error al crear el usuario.");

            return Ok(u);
        }

        [HttpPut("{id}")]
        public ActionResult<Usuario> Put(int id, [FromBody] UsuarioDto u)
        {
            var usuarioExistente = _repositorioUsuario.BuscarPorId(id);
            if (usuarioExistente == null) return NotFound();

            // 🔹 Solo actualizamos lo que permite el DTO
            usuarioExistente.email = u.Email;
            usuarioExistente.rol = u.Rol;
            usuarioExistente.estado = u.Estado;

            bool exito = _repositorioUsuario.Actualizar(usuarioExistente);
            if (!exito) return StatusCode(500, "Error al actualizar usuario.");

            return Ok(usuarioExistente);
        }


        [HttpDelete("{id}")]
        [Authorize(Policy = "Administrador")]
        public ActionResult Delete(int id)
        {
            bool exito = _repositorioUsuario.EliminadoLogico(id);
            if (!exito) return StatusCode(500, "Error al eliminar usuario.");
            return NoContent();
        }

        [HttpPatch("actualizar/pass/{id}")]
        public ActionResult<Usuario> CambiarPass(int id, [FromForm] string pass)
        {
            if (string.IsNullOrEmpty(pass) || pass.Length < 8)
                return BadRequest("La contraseña debe tener al menos 8 caracteres.");

            bool exito = _repositorioUsuario.CambiarPass(id, pass);
            if (!exito) return StatusCode(500, "Error al actualizar la contraseña.");

            var usuario = _repositorioUsuario.BuscarPorId(id);
            return Ok(usuario);
        }


        //PATCH http://localhost:5000/api/usuario/recuperar/pass

        [HttpPatch("recuperar/pass")]
        public ActionResult RecuperarPass([FromForm] string email, [FromForm] string documento, [FromForm] string nuevaPass)
        {
            if (string.IsNullOrEmpty(nuevaPass) || nuevaPass.Length < 8)
                return BadRequest("La contraseña debe tener al menos 8 caracteres.");

            bool exito = _repositorioUsuario.RecuperarPass(email, documento, nuevaPass);
            if (!exito) return NotFound("No se encontró un usuario con ese email y documento.");

            return Ok("Contraseña actualizada correctamente.");
        }

        
        [HttpPost("registrar-guardia")]
        [AllowAnonymous]
        public ActionResult RegistrarGuardia([FromForm] string documento, [FromForm] string email, [FromForm] string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return BadRequest("La contraseña debe tener al menos 8 caracteres.");

            bool exito = _repositorioUsuario.RegistrarGuardia(documento, email, password);
            if (!exito) return BadRequest("Error: guardia no encontrado, ya tiene usuario o datos inválidos.");

            return Ok("Usuario creado correctamente.");
        }


    }
}
