using KinoDev.StorageService.WebApi.Models.Configurations;
using KinoDev.StorageService.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace KinoDev.StorageService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
// Add autorization and other attributes as needed
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    private readonly BlobStorageSettings _blobStorageSettings;

    public FilesController(IFileService fileService, IOptions<BlobStorageSettings> blobStorageSettings)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _blobStorageSettings = blobStorageSettings?.Value ?? throw new ArgumentNullException(nameof(blobStorageSettings));
    }

    public class FileUploadRequest
    {
        public string FileName { get; set; }

        public string Base64Contents { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> UploadFileAsync([FromBody] FileUploadRequest request)
    {
        if (string.IsNullOrEmpty(request.FileName) || string.IsNullOrEmpty(request.Base64Contents))
        {
            return BadRequest("File name and contents must be provided.");
        }

        byte[] fileData;
        try
        {
            fileData = Convert.FromBase64String(request.Base64Contents);
        }
        catch (FormatException)
        {
            return BadRequest("Invalid Base64 string.");
        }

        var fileRelativePath = await _fileService.UploadPublicFileAsync(fileData, request.FileName, _blobStorageSettings.ContainerNames.PublicImages);
        return Ok(fileRelativePath);
    }

    [HttpGet("hello")]
    public IActionResult Hello()
    {
        return Ok("Hello from FilesController!");
    }
}