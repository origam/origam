using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class LocalizationController(LocalizationService localizationService) : ControllerBase
{
    [HttpPost("Rebuild")]
    public ActionResult Rebuild()
    {
        return Ok(localizationService.Rebuild());
    }
}
