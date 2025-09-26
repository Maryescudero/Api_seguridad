using System.Collections.Generic;

namespace Api_seguridad.Dtos
{
    public class FrancoDto
{
    public int idFranco { get; set; }
    public int idGuardia { get; set; }
    public string nombre { get; set; }
    public string apellido { get; set; }
    public string documento { get; set; }
    public string fechaFranco { get; set; }
    public string tipoFranco { get; set; }
}

}