using Api_seguridad.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositorios;
using Api_seguridad.Services; 

namespace Api_seguridad.Repositorios
{
    public class RepositorioUsuario : IRepositorio<Usuario>
    {
        private readonly DataContext _contexto;
        private readonly ILogger<RepositorioUsuario> _logger;

        public RepositorioUsuario(DataContext contexto, ILogger<RepositorioUsuario> logger)
        {
            _contexto = contexto;
            _logger = logger;
        }

        public bool Crear(Usuario usuario)
        {
            try
            {
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario nulo.");
                    return false;
                }

                // Hashear y guardar
                usuario.password = HashPass.HashearPass(usuario.password);
                _contexto.Usuarios.Add(usuario);
                return _contexto.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear usuario: {ex.Message}");
                return false;
            }
        }

        public bool Actualizar(Usuario usuario)
        {
            try
            {
                var existente = _contexto.Usuarios.FirstOrDefault(u => u.idUsuario == usuario.idUsuario);
                if (existente == null) return false;

                existente.email = usuario.email;
                existente.rol = usuario.rol;
                existente.estado = usuario.estado;
                return _contexto.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar usuario: {ex.Message}");
                return false;
            }
        }

        public Usuario BuscarPorId(int id)
        {
            return _contexto.Usuarios.FirstOrDefault(u => u.idUsuario == id);
        }

        public Usuario ObtenerPorEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return _contexto.Usuarios.FirstOrDefault(u => u.email.Trim().ToLower() == email.Trim().ToLower());
        }

        public Usuario? BuscarPorGuardia(int idGuardia)
        {
            return _contexto.Usuarios
                .AsNoTracking()
                .FirstOrDefault(u => u.idGuardia == idGuardia);
        }
        // Un solo admin (por compatibilidad con tu código actual)
        public Usuario? ObtenerAdministrador()
        {
            return _contexto.Usuarios
                .FirstOrDefault(u => u.rol == "administrador" && u.estado == true);
        }

        // Varios admins
        public List<Usuario> ObtenerAdministradores()
        {
            return _contexto.Usuarios
                .Where(u => u.rol == "administrador" && u.estado == true)
                .ToList();
        }




        public List<Usuario> ObtenerTodos()
        {
            return _contexto.Usuarios.ToList();
        }

        public bool EliminadoLogico(int id)
        {
            try
            {
                var usuario = _contexto.Usuarios.FirstOrDefault(u => u.idUsuario == id);
                if (usuario == null) return false;

                usuario.estado = false;
                return _contexto.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en eliminación lógica: {ex.Message}");
                return false;
            }
        }

        public bool CambiarPass(int usuarioID, string passwordNueva)
        {
            try
            {
                var usuario = _contexto.Usuarios.FirstOrDefault(u => u.idUsuario == usuarioID);
                if (usuario == null) return false;

                usuario.password = HashPass.HashearPass(passwordNueva);
                return _contexto.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al cambiar contraseña: {ex.Message}");
                return false;
            }
        }
        public Usuario ObtenerAdminPrincipal()
        {
            return _contexto.Usuarios.First(u => u.rol == "administrador");
        }

        public bool RecuperarPass(string email, string documento, string nuevaPass)
        {
            try
            {
                var usuario = (from u in _contexto.Usuarios
                               join g in _contexto.Guardias on u.idGuardia equals g.idGuardia
                               where u.email == email && g.documento == documento
                               select u).FirstOrDefault();

                if (usuario == null) return false;

                usuario.password = HashPass.HashearPass(nuevaPass);
                return _contexto.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al recuperar contraseña: {ex.Message}");
                return false;
            }
        }
        
        public bool RegistrarGuardia(string documento, string email, string password)
        {
            try
            {
                // Buscar guardia
                var guardia = _contexto.Guardias.FirstOrDefault(g => g.documento == documento);
                if (guardia == null) return false;

                // Validar si ya tiene usuario
                var existente = _contexto.Usuarios.FirstOrDefault(u => u.idGuardia == guardia.idGuardia);
                if (existente != null) return false;

                // Crear usuario asociado al guardia
                var nuevo = new Usuario
                {
                    idGuardia = guardia.idGuardia,
                    email = email,
                    password = HashPass.HashearPass(password),
                    rol = "guardia",
                    estado = true
                };

                _contexto.Usuarios.Add(nuevo);
                return _contexto.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al registrar guardia: {ex.Message}");
                return false;
            }
        }




    }
}
