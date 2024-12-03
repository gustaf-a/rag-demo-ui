using Microsoft.AspNetCore.Mvc;
using Shared.Plugins;

namespace AiDemos.Api.Controllers.RagDemo;

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
