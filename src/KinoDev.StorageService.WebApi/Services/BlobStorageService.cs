using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using KinoDev.StorageService.WebApi.Models.Configurations;
using Microsoft.Extensions.Options;

namespace KinoDev.StorageService.WebApi.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobStorageSettings _blobStorageSettings;

        public BlobStorageService(IOptions<BlobStorageSettings> blobStorageSettings)
        {
            _blobStorageSettings = blobStorageSettings.Value;
        }

        public async Task<Uri> Upload(byte[] bytes, string fileName, string containerName, PublicAccessType accessType = PublicAccessType.None)
        {
            try
            {
                var serviceClient = new BlobServiceClient(_blobStorageSettings.ConnectionString);

                var containerClient = serviceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync(accessType);

                var uploadBlob = containerClient.GetBlobClient(fileName);

                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    await uploadBlob.UploadAsync(ms, overwrite: true);
                }

                return uploadBlob.Uri;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}