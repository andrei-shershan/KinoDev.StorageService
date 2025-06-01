namespace KinoDev.StorageService.WebApi.Models.RequestModels
{
    public class FileUploadRequest
    {
        public string? FileName { get; set; }

        public string? Base64Contents { get; set; }
    }
}