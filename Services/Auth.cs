using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api_seguridad.Models;
using Microsoft.Extensions.Configuration;

namespace Api_seguridad.Services
{
    // Clase encargada de generar tokens JWT
    public class Auth
    {
        private readonly IConfiguration _config;

        public Auth(IConfiguration config)
        {
            _config = config;
        }

        // Genera un JWT  con claims 
        public string GenerarToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

            // Si no tiene guardia asociado, guardamos 0 que es para el administrador
            var idGuardiaClaim = usuario.idGuardia?.ToString() ?? "0";

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, usuario.email ?? string.Empty),
                new Claim(ClaimTypes.Role, usuario.rol ?? string.Empty), // Compatible con [Authorize(Roles = "...")]
                new Claim("rol", usuario.rol ?? string.Empty),
                new Claim("id", usuario.idUsuario.ToString()),          // id_usuario
                new Claim("id_guardia", idGuardiaClaim)                 // id_guardia o "0"
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(40),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
