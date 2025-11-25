using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class XsltController(
    XsltService xsltService,
    IWebHostEnvironment environment,
    ILogger<OrigamController> log
) : OrigamController(log, environment)
{
    [HttpPost("Validate")]
    public IActionResult Validate([FromBody] XsltValidateModel input)
    {
        return RunWithErrorHandler(() =>
        {
            Result result = xsltService.Validate(input.SchemaItemId);
            return Ok(result);
        });
    }
    
    [HttpPost("Transform")]
    public IActionResult Transform([FromBody] XsltTransformModel input)
    {
        return RunWithErrorHandler(() =>
        {
            Result result = xsltService.Transform(input.SchemaItemId);
            return Ok(result);
        });
    }
}
