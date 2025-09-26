using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_seguridad.Models
{
    [Table("historial_usuario")]
    public class HistorialUsuario
    {
        [Key]
        [Column("id_historial")]
        public int IdHistorial { get; set; }

        [Required]
        [Column("id_guardia")]
        public int idGuardia { get; set; }

        [Required]
        [Column("id_servicio")]
        public int idServicio { get; set; }

        [Required]
        [Column("fecha")]
        public DateOnly fecha { get; set; }

        [Column("ingreso")]
        public TimeOnly? ingreso { get; set; }   // opcional, hasta que marque ingreso

        [Column("egreso")]
        public TimeOnly? egreso { get; set; }    // opcional, hasta que marque egreso

        [Column("tipo")]
        [StringLength(20)]
        public string? tipo { get; set; }        // ej: asistencia, franco, ausente

        [Column("puntualidad")]
        [StringLength(20)]
        public string? puntualidad { get; set; } // ej: puntual, tarde, ausente

        [Column("observaciones")]
        [StringLength(255)]
        public string? observaciones { get; set; } // notas extra del supervisor

         // ✅ Propiedades de navegación
        [ForeignKey("idGuardia")]
        public Guardia? Guardia { get; set; }

        [ForeignKey("idServicio")]
        public Servicio? Servicio { get; set; }

    
        public override string ToString()
        {
            return $"GuardiaID: {idGuardia}, ServicioID: {idServicio}, Fecha: {fecha}, Tipo: {tipo}, Puntualidad: {puntualidad}, Ingreso: {ingreso}, Egreso: {egreso}, Observaciones: {observaciones}";
        }
    }
}

