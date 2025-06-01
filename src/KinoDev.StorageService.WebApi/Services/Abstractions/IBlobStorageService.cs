using Azure.Storage.Blobs.Models;

namespace KinoDev.StorageService.WebApi.Services.Abstractions
{
    public interface IBlobStorageService
    {
        Task<string> Upload(byte[] bytes, string fileName, string containerName, PublicAccessType accessType = PublicAccessType.None);
    }
}