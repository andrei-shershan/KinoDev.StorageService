using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KinoDev.StorageService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UpController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Up()
    {
        return Ok($"StorageService ::: Up at {DateTime.UtcNow}");
    }
}