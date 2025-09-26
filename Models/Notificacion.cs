using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_seguridad.Models
{
    [Table("notificacion")]
    public class Notificacion
    {
        [Key]
        [Column("id_notificacion")]  
        public int id_notificacion { get; set; }

        [Required]
        public string mensaje { get; set; } = string.Empty;

        public DateTime fecha_envio { get; set; } = DateTime.Now;

        // Quién envió la notificación (puede ser admin o guardia)
        public int? enviada_por { get; set; }

        // A quién va dirigida la notificación (admin o guardia)
        public int? enviada_a { get; set; }
         public bool leido { get; set; } 
    }
}
