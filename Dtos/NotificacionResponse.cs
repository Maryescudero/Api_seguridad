using System.Text.Json.Serialization;

namespace Api_seguridad.Dtos
{
    public class NotificacionResponse
    {
        [JsonPropertyName("id_notificacion")]
        public int IdNotificacion { get; set; }

        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; } = string.Empty;

        [JsonPropertyName("fecha_envio")]
        public string Fecha { get; set; } = string.Empty;

        [JsonPropertyName("enviada_por")]
        public int? EnviadaPor { get; set; }

        [JsonPropertyName("enviada_a")]
        public int? EnviadaA { get; set; }

        [JsonPropertyName("leido")]
        public bool Leido { get; set; }
    }
}
