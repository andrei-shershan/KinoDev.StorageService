using System.Text.RegularExpressions;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using KinoDev.StorageService.WebApi.Models.Configurations;
using KinoDev.StorageService.WebApi.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace KinoDev.StorageService.WebApi.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobStorageSettings _blobStorageSettings;

        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(
            IOptions<BlobStorageSettings> blobStorageSettings,
            ILogger<BlobStorageService> logger
            )
        {
            _blobStorageSettings = blobStorageSettings.Value;
            _logger = logger;
        }

        public async Task<string> Upload(byte[] bytes, string fileName, string containerName, PublicAccessType accessType = PublicAccessType.None)
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

                // Return related path without account storage name
                return Regex.Replace(uploadBlob.Uri.AbsolutePath, @"^/[^/]+/", "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file to Blob Storage. FileName: {FileName}, ContainerName: {ContainerName}", fileName, containerName);
                return null;
            }
        }
    }
}