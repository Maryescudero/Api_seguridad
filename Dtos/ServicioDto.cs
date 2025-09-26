using System.Collections.Generic;

namespace Api_seguridad.Dtos
{ 
    public class ServicioDto
    {
        public string lugar { get; set; }
        public string direccion { get; set; }
        public bool estado { get; set; }

       
        public List<TurnoConCantidadDto> turnos { get; set; } 
    }

    public class TurnoConCantidadDto
    {
        public int idTurno { get; set; }
        public int cantidadGuardias { get; set; }
    }
}
