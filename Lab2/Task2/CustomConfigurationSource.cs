using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Task1;
using Task1.Models;

namespace Task2;

public class CustomConfigurationSource : IConfigurationSource
{
    private readonly IServiceProvider _serviceProvider;

    public CustomConfigurationSource(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        CustomConfigurationProvider provider = _serviceProvider.GetRequiredService<CustomConfigurationProvider>();
        IConfigurationServiceClient client = _serviceProvider.GetRequiredService<IConfigurationServiceClient>();
        CustomConfigurationOptions options =
            _serviceProvider.GetRequiredService<IOptions<CustomConfigurationOptions>>().Value;

        IReadOnlyList<ConfigurationItem> initialConfigurations =
            client.GetAllConfigurationsAsync(CancellationToken.None).GetAwaiter().GetResult();

        var configDictionary = new Dictionary<string, string?>();
        foreach (ConfigurationItem configuration in initialConfigurations)
            configDictionary.Add(configuration.Key.Value, configuration.Value.Value);
        provider.UpdateConfigurations(configDictionary);

        var updateService = new ConfigurationUpdateService(provider, client, options.UpdateInterval);
        updateService.Start();

        return provider;
    }
}