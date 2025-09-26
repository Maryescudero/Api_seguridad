using QRCoder;

namespace Api_seguridad.Services
{
    public class QrCodeService
    {
        public byte[] GenerarPng(string contenido, int pixelsPerModule = 8)
        {
            using var gen = new QRCodeGenerator();
            using var data = gen.CreateQrCode(contenido, QRCodeGenerator.ECCLevel.Q);
            using var qr = new PngByteQRCode(data);
            return qr.GetGraphic(pixelsPerModule);
        }
    }
}
