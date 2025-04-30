using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.Wrappers;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ProjectController : ControllerBase
{
    private readonly ConfigManager configManager;
    private readonly Workbench workbench;

    public ProjectController(ConfigManager configManager, Workbench workbench)
    {
        this.configManager = configManager;
        this.workbench = workbench;
    }

    [HttpGet("[action]")]
    public IActionResult GetAvailable()
    {
        return Ok(configManager.Available);
    }    
    
    [HttpPost("[action]")]
    public IActionResult SetActive()
    {
        return Ok();
    }
}