using Microsoft.AspNetCore.Mvc;

namespace EntArtes.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "OK", timestamp = DateTime.UtcNow });
}
