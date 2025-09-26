using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_seguridad.Models
{
    [Table("turno_servicio")]
    public class TurnoServicio
    {
        [Column("id_servicio")]
        public int idServicio { get; set; }

        [Column("id_turno")]
        public int idTurno { get; set; }

        [Column("cant_guardias")]
        public int cantidadGuardias { get; set; }
        
        // Navegaciones a las entidades principales
        public Servicio Servicio { get; set; } = null!;
        public Turno Turno { get; set; } = null!;
        
    }
}