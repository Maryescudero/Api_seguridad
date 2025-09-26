using QuestPDF.Infrastructure;

namespace Api_seguridad
{
    public static class QuestPdfConfig
    {
        static QuestPdfConfig()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public static void Initialize()
        {
            // Método vacío que asegura la ejecución del constructor estático
        }
    }
}
