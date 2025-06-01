namespace KinoDev.StorageService.WebApi.Services.Abstractions
{
    public interface IQRCodeService
    {
        string GenerateQRCodeInBase64Async(string text);
    }
}