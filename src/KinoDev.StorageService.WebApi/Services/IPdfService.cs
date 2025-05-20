namespace KinoDev.StorageService.WebApi.Services
{
    public interface IPdfService
    {
        Task<byte[]> GeneratePdfAsync(string htmlContent);
    }        
}