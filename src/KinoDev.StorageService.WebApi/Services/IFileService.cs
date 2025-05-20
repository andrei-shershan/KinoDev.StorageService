using KinoDev.Shared.DtoModels.Orders;

namespace KinoDev.StorageService.WebApi.Services
{
    public interface IFileService
    {
        Task GenerateAndUploadFileAsync(OrderSummary orderSummary, CancellationToken cancellationToken);
    }
}