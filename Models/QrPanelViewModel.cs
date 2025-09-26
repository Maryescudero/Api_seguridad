// Models/QrPanelViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Api_seguridad.Models
{
    public class QrPanelViewModel
    {
        public int IdServicio { get; set; }
        public int IdTurno { get; set; }
        public DateOnly Fecha { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        public int RefreshSeconds { get; set; } = 30;
        public string? ServicioTexto { get; set; }
        public string? TurnoTexto { get; set; }


        public List<SelectListItem> Servicios { get; set; } = new();
        public List<SelectListItem> Turnos { get; set; } = new();
    }
}
