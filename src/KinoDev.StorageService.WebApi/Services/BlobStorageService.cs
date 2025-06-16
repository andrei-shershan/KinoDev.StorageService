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
                    _logger.LogInformation("Uploading file to Blob Storage. FileName: {FileName}, ContainerName: {ContainerName}", fileName, containerName);
                    await uploadBlob.UploadAsync(ms, overwrite: true);
                }

                // Return related path without account storage name
                _logger.LogInformation("File uploaded successfully. FileName: {FileName}, ContainerName: {ContainerName}", fileName, containerName);
                _logger.LogInformation("File URI: {FileUri}", uploadBlob.Uri.AbsolutePath);
                var ifExist = await uploadBlob.ExistsAsync();
                _logger.LogInformation("File exists: {IfExist}", ifExist);
                var replaced = Regex.Replace(uploadBlob.Uri.AbsolutePath, @"^/[^/]+/", "");
                _logger.LogInformation("Replaced path: {Replaced}", replaced);
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