namespace Api_seguridad.Dtos
{
    public class AsignacionResponseDto
    {
        public bool Ok { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
        public object? Data { get; set; } // opcional, si querés devolver algo extra
    }
}
