using Microsoft.AspNetCore.Mvc;
using RagDemoAPI.Plugins;

namespace RagDemoAPI.Controllers;

/// <summary>
/// A controller for the available plugins
/// </summary>
[ApiController]
[Route("plugins")]
public class PluginsController(ILogger<PluginsController> _logger, IEnumerable<IPlugin> _plugins) : ControllerBase
{
    [HttpGet("get-names")]
    public async Task<IActionResult> GetPluginNames()
    {
        var pluginNames = _plugins.Select(plugin => plugin.GetType().Name).ToList();

        return Ok(pluginNames);
    }
}
