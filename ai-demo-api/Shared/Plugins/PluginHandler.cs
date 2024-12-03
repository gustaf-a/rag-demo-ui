using Microsoft.SemanticKernel;

namespace Shared.Plugins;

public class PluginHandler(IServiceProvider _serviceProvider) : IPluginHandler
{
    private static readonly List<string> _defaultPlugins = [nameof(DatePlugin), nameof(TimePlugin), nameof(SearchDatabasePlugin)];

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
                case nameof(SearchDatabasePlugin):
                    ////TODO Fel. Hur ska jag göra ? Funkar inte.
                    //var builder = Kernel.CreateBuilder();
                    //builder.Services.AddScoped<ISearchServiceFactory, SearchServiceFactory>();
                    //var newKernel = builder.Build();

                    //foreach (var plugin in kernel.Plugins)
                    //    newKernel.Plugins.Add(plugin);

                    kernel.Plugins.AddFromType<SearchDatabasePlugin>(serviceProvider: _serviceProvider);

                    //kernel = newKernel;
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
