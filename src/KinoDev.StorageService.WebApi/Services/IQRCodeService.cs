namespace KinoDev.StorageService.WebApi.Services
{
    public interface IQRCodeService
    {
        string GenerateQRCodeInBase64Async(string text);
    }
}