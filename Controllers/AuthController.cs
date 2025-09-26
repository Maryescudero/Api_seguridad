// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Api_seguridad.Models;
using Api_seguridad.Repositorios;
using Api_seguridad.Services;

namespace Api_seguridad.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RepositorioUsuario _repoUsuario;
        private readonly Auth _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(RepositorioUsuario repoUsuario, Auth authService, ILogger<AuthController> logger)
        {
            _repoUsuario = repoUsuario;
            _authService = authService;
            _logger = logger;
        }

        //http://localhost:5000/api/auth/login
        [HttpPost("login")]   
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginModel login)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = _repoUsuario.ObtenerPorEmail(login.email!);
            if (usuario == null || !usuario.estado)
                return Unauthorized("Usuario inexistente o inactivo.");

            bool okPass = HashPass.VerificarPassword(login.password!, usuario.password);
            if (!okPass)
                return Unauthorized("Contraseña incorrecta.");

            var token = _authService.GenerarToken(usuario);

            return Ok(new
            {
                token,
                usuario = new
                {
                    idUsuario = usuario.idUsuario,
                    idGuardia = usuario.idGuardia,  // null para admin
                    email = usuario.email,
                    rol = usuario.rol
                }
            });
        }
    }

}
