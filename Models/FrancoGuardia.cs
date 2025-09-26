using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_seguridad.Models
{
    [Table("franco_guardia")]
    public class FrancoGuardia
    {
        [Key]
        [Column("id_franco")]
        public int idFranco { get; set; }

        [Required]
        [Column("id_guardia")]
        public int idGuardia { get; set; }

        [Required]
        [Column("fecha_franco")]
        public DateOnly fechaFranco { get; set; }

        [Required]
        [Column("tipo_franco")]
        public string tipoFranco { get; set; }

        [ForeignKey("idGuardia")]
        public Guardia Guardia { get; set; }

    }
}
