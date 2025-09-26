using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Api_seguridad.DTOs;

namespace Api_seguridad.Reportes
{
    public class ReporteConsolidadoPdf
    {
        public static byte[] Generar(List<ResumenMensualConsolidadoDto> datos, int mes, int anio)
        {
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);

                    //  Encabezado
                    page.Header().Text($"📊 Reporte Mensual Consolidado - {mes}/{anio}")
                                 .FontSize(18).Bold().AlignCenter();

                    //  Tabla
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {

                            columns.RelativeColumn();     // Nombre
                            columns.RelativeColumn();     // Apellido
                            columns.RelativeColumn();
                            columns.RelativeColumn();     // Asignados
                            columns.RelativeColumn();     // Cubiertos
                            columns.RelativeColumn();     // Horas Diurnas
                            columns.RelativeColumn();     // Horas Nocturnas
                            columns.RelativeColumn();     // Horas Totales
                        });

                        // Cabeceras
                        table.Header(header =>
                        {

                            header.Cell().Element(CellStyle).Text("Documento");
                            header.Cell().Element(CellStyle).Text("Nombre");
                            header.Cell().Element(CellStyle).Text("Apellido");    
                            header.Cell().Element(CellStyle).Text("Asignados");
                            header.Cell().Element(CellStyle).Text("Cubiertos");
                            header.Cell().Element(CellStyle).Text("Horas Diurnas");
                            header.Cell().Element(CellStyle).Text("Horas Nocturnas");
                            header.Cell().Element(CellStyle).Text("Horas Totales");
                        });

                        // Filas
                        foreach (var item in datos)
                        {

                            table.Cell().Element(CellStyle).Text(item.Documento);
                            table.Cell().Element(CellStyle).Text(item.Nombre);
                            table.Cell().Element(CellStyle).Text(item.Apellido);
                            table.Cell().Element(CellStyle).Text(item.TotalServiciosAsignados.ToString());
                            table.Cell().Element(CellStyle).Text(item.TotalServiciosCubiertos.ToString());
                            table.Cell().Element(CellStyle).Text(item.HorasDiurnas);
                            table.Cell().Element(CellStyle).Text(item.HorasNocturnas);
                            table.Cell().Element(CellStyle).Text(item.HorasTotales);
                        }
                    });

                    //  Pie de pag
                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.Span("Generado el ").Italic();
                        txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                    });
                });
            });

            return documento.GeneratePdf();
        }

        //  Estilo para celdas
        private static IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .PaddingVertical(2)
                .AlignCenter()
                .AlignMiddle();
        }
    }
}
