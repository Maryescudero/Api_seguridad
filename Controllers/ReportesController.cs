using Microsoft.AspNetCore.Mvc;
using Api_seguridad.Repositorios;
using Api_seguridad.Reportes;

namespace Api_seguridad.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly RepositorioReporte _repositorio;

        public ReportesController(RepositorioReporte repositorio)
        {
            _repositorio = repositorio;
        }
       // http://localhost:5000/api/reportes/mensual-consolidado?mes=9&anio=2025
        [HttpGet("mensual-consolidado")]
        public IActionResult ReporteMensualConsolidadoPdf([FromQuery] int mes, [FromQuery] int anio)
        {
            var datos = _repositorio.ObtenerResumenMensual(mes, anio);
            var pdfBytes = ReporteConsolidadoPdf.Generar(datos, mes, anio);

            return File(pdfBytes, "application/pdf", $"reporte_mensual_{mes}_{anio}.pdf");
        }
    }
}
