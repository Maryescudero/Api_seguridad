using System.Collections.Generic;

namespace Api_seguridad.Dtos
{
    public class NotificacionRequest
    {
        public string Mensaje { get; set; } = string.Empty;
        public int? EnviadaPor { get; set; }
        public int? EnviadaA { get; set; }
        public string RolDestino { get; set; } = "admin";
    }
}