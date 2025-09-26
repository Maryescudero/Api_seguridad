using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Api_seguridad.Models
{
    [Table("usuario")]
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public int idUsuario { get; set; }

        [Column("id_guardia")]
        public int? idGuardia { get; set; } // null para administradores

        [Required]
        [EmailAddress]
        [Column("email")]
        public string email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string password { get; set; } = null!; // Guardar hash, no texto plano

        [Required]
        [RegularExpression("^(guardia|administrador)$", ErrorMessage = "El rol debe ser 'guardia' o 'administrador'.")]
        public string rol { get; set; } = null!;

        public bool estado { get; set; }

        public override string ToString()
        {
            return $"{idUsuario} | {idGuardia}  | {email} |{ rol} | {estado}";
        }
    }
}
