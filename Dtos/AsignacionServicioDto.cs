using System;
using System.Collections.Generic;

namespace Api_seguridad.Dtos
{
    public class AsignacionServicioDto
    {
        public int id_asignacionServicio { get; set; }
        public int id_guardia { get; set; }
        public int id_servicio { get; set; }
        public int id_turno { get; set; }
        public DateOnly fecha { get; set; }
        public bool estado { get; set; }


        // Servicio
        public string lugar { get; set; }
        public string direccion { get; set; }

        // Turno
        public string nombreTurno { get; set; }
        public string horario { get; set; } // "HH:mm - HH:mm"

        // Guardia
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string documento { get; set; }

        // Historial (si existe)
        public string puntualidad { get; set; }   // puntual | tardanza | null
        public string ingreso { get; set; }       // "HH:mm" | "" 
        public string egreso { get; set; }        // "HH:mm" | ""
        public bool cumplido { get; set; }        // ingreso y egreso presentes
    }
}


