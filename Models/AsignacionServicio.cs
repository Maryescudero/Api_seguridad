using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_seguridad.Models
{
    [Table("asignacion_servicio")]
    public class AsignacionServicio
    {
        [Key]
        [Column("id_asignacionServicio")]
        public int idAsignacionServicio { get; set; }

        [Required]
        [Column("id_guardia")]
        public int idGuardia { get; set; }

        [Required]
        [Column("id_servicio")]
        public int idServicio { get; set; }

        [Required]
        [Column("id_turno")]
        public int idTurno { get; set; }

        [Required]
        [Column("fecha_asignacion")]
        public DateOnly fechaAsignacion { get; set; }

        [Required]
        [Column("estado")]
        public bool estado { get; set; } = true; 

    }
}
