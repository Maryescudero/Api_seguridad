using Api_seguridad.Models;
using Repositorios;

namespace Api_seguridad.Repositorios
{
    public class RepositorioTurno : IRepositorio<Turno>
    {
        private readonly DataContext _contexto;

        public RepositorioTurno(DataContext contexto)
        {
            _contexto = contexto;
        }

        public List<Turno> ObtenerTodos()
        {
            return _contexto.Turnos.ToList();
        }

        public Turno BuscarPorId(int id)
        {
            return _contexto.Turnos.FirstOrDefault(t => t.idTurno == id);
        }

        public bool Crear(Turno entity)
        {
            _contexto.Turnos.Add(entity);
            return _contexto.SaveChanges() > 0;
        }

        public bool Actualizar(Turno entity)
        {
            var existente = _contexto.Turnos.Find(entity.idTurno);
            if (existente == null) return false;

            existente.nombre = entity.nombre;
            existente.horaInicio = entity.horaInicio;
            existente.horaFin = entity.horaFin;
            return _contexto.SaveChanges() > 0;
        }

        public bool EliminadoLogico(int id)
        {
            // Si tu lógica de turno permite borrado (poco usual), si no, no implementes.
            return false;
        }

        public List<Turno> ObtenerActivos()
        {
            // Si tienes un campo de estado en turno, filtrá por él; si no, devolvé todos.
            return _contexto.Turnos.ToList();
        }
    }
}
