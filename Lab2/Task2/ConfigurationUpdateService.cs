using Task1;
using Task1.Models;

namespace Task2;

public class ConfigurationUpdateService : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly CustomConfigurationProvider _provider;
    private readonly IConfigurationServiceClient _client;
    private readonly TimeSpan _updateInterval;
    private PeriodicTimer? _timer;

    public ConfigurationUpdateService(
        CustomConfigurationProvider provider,
        IConfigurationServiceClient client,
        TimeSpan updateInterval)
    {
        _provider = provider;
        _client = client;
        _updateInterval = updateInterval;
    }

    public void Start()
    {
        Console.WriteLine("ConfigurationUpdateService запущен с интервалом обновления: " + _updateInterval);

        _timer = new PeriodicTimer(_updateInterval);
        Task.Run(async () =>
        {
            while (await _timer.WaitForNextTickAsync())
            {
                await UpdateConfigurationsAsync();
            }
        });
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }

    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
        _timer?.Dispose();
    }

    private async Task UpdateConfigurationsAsync()
    {
        IReadOnlyList<ConfigurationItem> configurations =
            await _client.GetAllConfigurationsAsync(CancellationToken.None);

        var configDictionary = configurations.ToDictionary(
            config => config.Key.Value,
            string? (config) => config.Value.Value);
        _provider.UpdateConfigurations(configDictionary);
    }
}