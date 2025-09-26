using Api_seguridad.Models;

namespace Api_seguridad.Repositorios
{
    public class RepositorioTurnoServicio
    {
        private readonly DataContext _ctx;

        public RepositorioTurnoServicio(DataContext ctx)
        {
            _ctx = ctx;
        }

        public bool ExisteTurnoParaServicio(int idServicio, int idTurno)
        {
            return _ctx.TurnoServicios.Any(ts => ts.idServicio == idServicio && ts.idTurno == idTurno);
        }
    }
}
