using Microsoft.SemanticKernel;

namespace Shared.Plugins;

public class PluginHandler(IServiceProvider _serviceProvider) : IPluginHandler
{
    private static readonly List<string> _defaultPlugins = [nameof(DatePlugin), nameof(TimePlugin), nameof(SearchDatabasePlugin), nameof(MathPlugin), nameof(FilePlugin)];

    public void AddPlugins(Kernel kernel)
    {
        AddPlugins(kernel, _defaultPlugins);
    }

    public void AddPlugins(Kernel kernel, IEnumerable<string> pluginNames)
    {
        foreach (var pluginName in pluginNames)
        {
            switch (pluginName)
            {
                case nameof(DatePlugin):
                    kernel.Plugins.AddFromType<DatePlugin>();
                    break;
                case nameof(FilePlugin):
                    kernel.Plugins.AddFromType<FilePlugin>(serviceProvider: _serviceProvider);
                    break;
                case nameof(MathPlugin):
                    kernel.Plugins.AddFromType<MathPlugin>();
                    break;
                case nameof(SearchDatabasePlugin):
                    kernel.Plugins.AddFromType<SearchDatabasePlugin>(serviceProvider: _serviceProvider);
                    break;
                case nameof(TimePlugin):
                    kernel.Plugins.AddFromType<TimePlugin>();
                    break;

                default:
                    throw new NotImplementedException($"Plugin name not added to {nameof(PluginHandler)}: {pluginName}. Make sure to also add in kernel service injection.");
            }
        }
    }
}
