using QRCoder;

namespace EventPulse.Services
{
    public interface IQrCodeService
    {
        byte[] GeneratePng(string payload, int pixelsPerModule = 8);
    }

    /// <summary>
    /// Turns a registration's unique QR payload into a PNG. The payload is the
    /// same Guid string stored on Registration.QrCode so a scan matches CheckIns.
    /// </summary>
    public class QrCodeService : IQrCodeService
    {
        public byte[] GeneratePng(string payload, int pixelsPerModule = 8)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            var png = new PngByteQRCode(data);
            return png.GetGraphic(pixelsPerModule);
        }
    }
}
