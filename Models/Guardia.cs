using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_seguridad.Models
{
    [Table("guardia")]
    public class Guardia
    {
        [Key]
        [Column("id_guardia")]
        public int idGuardia { get; set; }

        [Required]
        [Column("nombre")]
        public string nombre { get; set; }

        [Required]
        [Column("apellido")]
        public string apellido { get; set; }

        [Required]
        [Column("documento")]
        public string documento { get; set; }

        [Required]
        [Column("direccion")]
        public string direccion { get; set; }

        [Required]
        [Column("telefono")]
        public string telefono { get; set; }

        [Required]
        [Column("alta")]
        public DateOnly alta { get; set; }

        [Column("estado")]
        public bool estado { get; set; }

        [Required]
        [Column("tipoGuardia")]
        public string tipoGuardia { get; set; }

        public override string ToString()
        {
            return $"ID: {idGuardia}, {apellido}, {nombre}, Documento: {documento}, Dirección: {direccion}, Teléfono: {telefono}, Alta: {alta}, Estado: {(estado ? "Activo" : "Inactivo")}";
        }

        public string ToStringWeb()
        {
            return $"{apellido}, {nombre} - Documento: {documento}";
        }
    }
}
