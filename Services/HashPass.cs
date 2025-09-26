//hashpass
using BCrypt.Net;
namespace Api_seguridad.Services
{
     // Clase estática que encapsula hashing y verificación de passwords con BCrypt
    public static class HashPass
    {
        // Método para hashear la contraseña
        public static string HashearPass(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Método para verificar la contraseña contra el hash almacenado
        public static bool VerificarPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}