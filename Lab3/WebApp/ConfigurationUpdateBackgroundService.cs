using Microsoft.Extensions.Options;
using Task1;
using Task2;

namespace WebApp;

public class ConfigurationUpdateBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CustomConfigurationProvider _provider;
    private readonly TimeSpan _updateInterval;

    public ConfigurationUpdateBackgroundService(
        IServiceProvider serviceProvider,
        CustomConfigurationProvider provider,
        IOptions<CustomConfigurationOptions> options)
    {
        _serviceProvider = serviceProvider;
        _provider = provider;
        _updateInterval = options.Value.UpdateInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await UpdateConfigurationsAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_updateInterval, stoppingToken);
            await UpdateConfigurationsAsync(stoppingToken);
        }
    }

    private async Task UpdateConfigurationsAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        IConfigurationServiceClient client = scope.ServiceProvider.GetRequiredService<IConfigurationServiceClient>();
        IReadOnlyList<Task1.Models.ConfigurationItem> configurations = await client.GetAllConfigurationsAsync(cancellationToken);
        var configDictionary = new Dictionary<string, string?>();
        foreach (Task1.Models.ConfigurationItem configuration in configurations) configDictionary.Add(configuration.Key.Value, configuration.Value.Value);
        _provider.UpdateConfigurations(configDictionary);
    }
}