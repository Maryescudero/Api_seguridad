using System.Collections.Generic;

namespace Api_seguridad.Dtos
{
    public class UsuarioDto
    {
        public string Email { get; set; }
        public string Rol { get; set; }
        public bool Estado { get; set; }
    }
}