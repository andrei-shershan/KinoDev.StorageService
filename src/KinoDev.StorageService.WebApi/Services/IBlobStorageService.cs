using Azure.Storage.Blobs.Models;

namespace KinoDev.StorageService.WebApi.Services
{
    public interface IBlobStorageService
    {
        Task<Uri> Upload(byte[] bytes, string fileName, string containerName, PublicAccessType accessType = PublicAccessType.None);
    }
}