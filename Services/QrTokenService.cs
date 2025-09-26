using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api_seguridad.Services
{
    public class QrTokenService
    {
        private readonly IConfiguration _cfg;
        public QrTokenService(IConfiguration cfg) => _cfg = cfg;

        public string GenerarTokenQr(int idServicio, int idTurno, DateOnly fecha, string tipo) // "ingreso" | "egreso"
        {
            var ttl = int.TryParse(_cfg["Qr:TtlSeconds"], out var s) ? s : 45;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Qr:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new("svc", idServicio.ToString()),
                new("turno", idTurno.ToString()),
                new("fecha", fecha.ToString("yyyy-MM-dd")),
                new("tipo", tipo),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                claims: claims,
                notBefore: DateTime.UtcNow.AddSeconds(-5),
                expires: DateTime.UtcNow.AddSeconds(ttl),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (bool ok, int idServicio, int idTurno, DateOnly fecha, string tipo, string? error) Validar(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Qr:Key"]!));
            try
            {
                var p = new TokenValidationParameters
                {
                    ValidateIssuer = false, ValidateAudience = false,
                    ValidateIssuerSigningKey = true, IssuerSigningKey = key,
                    ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(3)
                };
                var principal = handler.ValidateToken(token, p, out _);
                int idServicio = int.Parse(principal.FindFirst("svc")!.Value);
                int idTurno    = int.Parse(principal.FindFirst("turno")!.Value);
                var fecha      = DateOnly.Parse(principal.FindFirst("fecha")!.Value);
                var tipo       = principal.FindFirst("tipo")!.Value;
                return (true, idServicio, idTurno, fecha, tipo, null);
            }
            catch (Exception ex) { return (false, 0, 0, default, "", ex.Message); }
        }
    }
}
