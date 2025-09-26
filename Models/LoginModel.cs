using System.ComponentModel.DataAnnotations;

namespace Api_seguridad.Models
{
    public class LoginModel
    {
        [Required, EmailAddress]
        public string email { get; set; } = null!;

        [Required, MinLength(8)]
        public string password { get; set; } = null!;
    }
}
