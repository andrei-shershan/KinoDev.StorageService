using KinoDev.StorageService.WebApi.Services.Abstractions;
using QRCoder;

namespace KinoDev.StorageService.WebApi.Services
{
    public class QRCodeService : IQRCodeService
    {
        private const int pixelsPerModule = 20;

        public string GenerateQRCodeInBase64Async(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);

            var qrCodeBytes = qrCode.GetGraphic(pixelsPerModule);
            return Convert.ToBase64String(qrCodeBytes);
        }
    }
}