// Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Api_seguridad.Models;
using Api_seguridad.Repositorios;

namespace Api_seguridad.Controllers
{
    public class HomeController : Controller
    {
        private readonly RepositorioServicio _repoServicio;
        private readonly RepositorioTurno _repoTurno; // por si no hay mapeo
        private readonly ILogger<HomeController> _logger;

        public HomeController(RepositorioServicio repoServicio,
                              RepositorioTurno repoTurno,
                              ILogger<HomeController> logger)
        {
            _repoServicio = repoServicio;
            _repoTurno = repoTurno;
            _logger = logger;
        }

        public IActionResult Index(int idServicio = 1, int idTurno = 1, int refresh = 30, DateOnly? fecha = null)
{
    var servicios = _repoServicio.ObtenerActivosOrdenados();
    if (!servicios.Any())
        return Content("No hay servicios activos cargados.");

    if (!servicios.Any(s => s.idServicio == idServicio))
        idServicio = servicios.First().idServicio;

    var turnos = _repoServicio.ObtenerTurnosPorServicio(idServicio);
    if (!turnos.Any()) turnos = _repoTurno.ObtenerTodos();
    if (!turnos.Any()) return Content("No hay turnos definidos.");

    if (!turnos.Any(t => t.idTurno == idTurno))
        idTurno = turnos.First().idTurno;

    var vm = new QrPanelViewModel
    {
        IdServicio = idServicio,
        IdTurno = idTurno,
        RefreshSeconds = refresh,
        Fecha = fecha ?? DateOnly.FromDateTime(DateTime.Today),

        Servicios = servicios.Select(s => new SelectListItem
        {
            Value = s.idServicio.ToString(),
            Text = $"{s.lugar} - {s.direccion}",
            Selected = (s.idServicio == idServicio)
        }).ToList(),

        Turnos = turnos.Select(t => new SelectListItem
        {
            Value = t.idTurno.ToString(),
            Text = $"{t.nombre} ({t.horaInicio:HH\\:mm}-{t.horaFin:HH\\:mm})",
            Selected = (t.idTurno == idTurno)
        }).ToList()
    };

    // muestra lo seleccionado
    var servSel  = servicios.First(s => s.idServicio == idServicio);
    var turnoSel = turnos.First(t => t.idTurno == idTurno);

    vm.ServicioTexto = $"{servSel.lugar} ";
    vm.TurnoTexto    = $"{turnoSel.nombre}";

    return View(vm);
}

    }
}
