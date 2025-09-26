using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_seguridad.Models
{
    [Table("turno")]
    public class Turno
    {
        [Key]
        [Column("id_turno")]
        public int idTurno { get; set; }

        [Required]
        [Column("nombre")]
        public string nombre { get; set; }

        [Required]
        [Column("hora_inicio")]
        public TimeOnly horaInicio { get; set; }

        [Required]
        [Column("hora_fin")]
        public TimeOnly horaFin { get; set; }
        


        public ICollection<TurnoServicio> TurnoServicios { get; set; } = new List<TurnoServicio>();
    
    }
}
