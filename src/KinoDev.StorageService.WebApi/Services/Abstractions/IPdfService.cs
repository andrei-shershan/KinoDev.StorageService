namespace KinoDev.StorageService.WebApi.Services.Abstractions
{
    public interface IPdfService
    {
        Task<byte[]> GeneratePdfAsync(string htmlContent);
    }        
}