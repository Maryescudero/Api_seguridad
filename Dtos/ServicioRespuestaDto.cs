using System.Collections.Generic;

namespace Api_seguridad.Dtos
{
    public class ServicioRespuestaDto
    {
        public int idServicio { get; set; }
        public string lugar { get; set; }
        public string direccion { get; set; }
        public DateOnly fechaAlta { get; set; }
        public bool estado { get; set; }
        public List<TurnoDto> turnos { get; set; }
        public List<int> id_turnos { get; set; } = new List<int>();
    }

     public class TurnoDto
    {
        public int idTurno { get; set; }
        public string nombre { get; set; }
        public string hora_inicio { get; set; }
        public string hora_fin { get; set; }
    }


}