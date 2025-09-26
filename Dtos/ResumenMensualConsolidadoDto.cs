namespace Api_seguridad.DTOs
{
    public class ResumenMensualConsolidadoDto
    {
        public int Guardia { get; set; }        // ID interno (oculto en PDF)
        public string Documento { get; set; }   // Documento visible en PDF
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public string HorasDiurnas { get; set; }   // ej: "120h"
        public string HorasNocturnas { get; set; } // ej: "40h"
        public string HorasTotales { get; set; }   // ej: "160h"
        public int TotalServiciosAsignados { get; set; }
        public int TotalServiciosCubiertos { get; set; }
    }
}
