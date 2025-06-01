using KinoDev.Shared.DtoModels.Orders;

namespace KinoDev.StorageService.WebApi.Services.Abstractions
{
    public interface IFileService
    {
        Task GenerateAndUploadFileAsync(OrderSummary orderSummary, CancellationToken cancellationToken);

        Task<string> UploadPublicFileAsync(byte[] bytes, string fileName, string containerName);
    }
}