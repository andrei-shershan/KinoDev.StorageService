using Azure.Storage.Blobs.Models;
using KinoDev.Shared.DtoModels.Orders;

namespace KinoDev.StorageService.WebApi.Services
{
    public interface IFileService
    {
        Task GenerateAndUploadFileAsync(OrderSummary orderSummary, CancellationToken cancellationToken);

        Task<string> UploadPublicFileAsync(byte[] bytes, string fileName, string containerName);
    }
}