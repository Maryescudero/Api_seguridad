using Api_seguridad.Models;
using Microsoft.EntityFrameworkCore;
using Repositorios;

namespace Api_seguridad.Repositorios
{
    public class RepositorioGuardia
    {
        private readonly DataContext _contexto;
        private readonly ILogger<RepositorioGuardia> _logger;

        public RepositorioGuardia(DataContext contexto, ILogger<RepositorioGuardia> logger)
        {
            _contexto = contexto;
            _logger = logger;
        }

        public bool Crear(Guardia guardia)
        {
            try
            {
                if (guardia == null) return false;

                // Verificar si ya existe por documento
                var existente = _contexto.Guardias.FirstOrDefault(g => g.documento == guardia.documento);

                if (existente != null)
                {
                    if (!existente.estado)
                    {
                        // Reactivar y actualizar datos
                        existente.nombre = guardia.nombre;
                        existente.apellido = guardia.apellido;
                        existente.direccion = guardia.direccion;
                        existente.telefono = guardia.telefono;
                        existente.alta = guardia.alta;
                        existente.estado = true;

                        return _contexto.SaveChanges() > 0;
                    }
                    else
                    {
                        // Ya existe activo, no crear duplicado
                        return false;
                    }
                }

                // No existe, crear nuevo
                _contexto.Guardias.Add(guardia);
                return _contexto.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear guardia: {ex.Message}");
                return false;
            }
        }

        public bool Actualizar(Guardia guardia)
        {
            try
            {
                var existente = _contexto.Guardias.FirstOrDefault(g => g.idGuardia == guardia.idGuardia);
                if (existente == null) return false;

                existente.nombre = guardia.nombre;
                existente.apellido = guardia.apellido;
                existente.documento = guardia.documento;
                existente.direccion = guardia.direccion;
                existente.telefono = guardia.telefono;
                existente.alta = guardia.alta;
                existente.estado = guardia.estado;

                return _contexto.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar guardia: {ex.Message}");
                return false;
            }
        }

        public Guardia BuscarPorId(int id)
        {
            return _contexto.Guardias.FirstOrDefault(g => g.idGuardia == id);
        }

        public List<Guardia> ObtenerTodos()
        {
            return _contexto.Guardias.ToList();
        }

        public List<Guardia> ObtenerActivos()
        {
            return _contexto.Guardias.Where(g => g.estado).ToList();
        }

        public bool EliminadoLogico(int id)
        {
            try
            {
                var guardia = _contexto.Guardias.FirstOrDefault(g => g.idGuardia == id);
                if (guardia == null) return false;

                guardia.estado = false;
                return _contexto.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar lógicamente guardia: {ex.Message}");
                return false;
            }
        }

        public Guardia BuscarPorDocumento(string documento)
        {
            return _contexto.Guardias.FirstOrDefault(g => g.documento == documento);
        }
    }
}
