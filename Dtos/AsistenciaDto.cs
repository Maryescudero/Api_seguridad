namespace Api_seguridad.Dtos
{
    public class AsistenciaDto
    {
        public int idHistorial { get; set; }
        public int idGuardia { get; set; }
        public string nombreGuardia { get; set; }
        public string apellidoGuardia { get; set; }
        public int idServicio { get; set; }
        public string lugarServicio { get; set; }
        public DateOnly fecha { get; set; }
        public string tipo { get; set; }          // asistencia | ausente | franco
        public string? puntualidad { get; set; }  // puntual | tardanza | null
        public string? ingreso { get; set; }      // "HH:mm" o null
        public string? egreso { get; set; }       // "HH:mm" o null
        public string? observaciones { get; set; }
         public Double horasTrabajadas { get; set; }   // total de horas (0 si no tiene egreso)
    }
}
