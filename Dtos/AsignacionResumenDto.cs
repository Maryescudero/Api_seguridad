using System.Collections.Generic;

namespace Api_seguridad.Dtos
{
    public class AsignacionesResumenDto
    {
        public List<AsignacionServicioDto> asignaciones { get; set; }
        public int totalAsignaciones { get; set; }
        public int guardiasUtilizados { get; set; }
        public int serviciosCubiertos { get; set; }
    }
}
