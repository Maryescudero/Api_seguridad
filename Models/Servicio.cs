using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_seguridad.Models
{
    [Table("servicio")]
    public class Servicio
    {
        [Key]
        [Column("id_servicio")]
        public int idServicio { get; set; }

        [Required]
        [Column("lugar")]
        public string lugar { get; set; }

        [Required]
        [Column("direccion")]
        public string direccion { get; set; }

        [Required]
        [Column("fecha_alta")]
        public DateOnly fechaAlta { get; set; }

        [Column("estado")]
        public bool estado { get; set; }

         public ICollection<TurnoServicio> TurnoServicios { get; set; } = new List<TurnoServicio>();

        public override string ToString()
        {
            return $"{idServicio} - {lugar} ({direccion}) -  {fechaAlta.ToShortDateString()} - Estado: {(estado ? "Activo" : "Inactivo")}";
        }
    }
}
