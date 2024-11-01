using Microsoft.Extensions.Configuration;

namespace Task2;

public class CustomConfigurationProvider : ConfigurationProvider
{
    private Dictionary<string, string?> _lastConfigurations;

    public CustomConfigurationProvider()
    {
        _lastConfigurations = new Dictionary<string, string?>();
    }

    public virtual void UpdateConfigurations(Dictionary<string, string?> newConfigurations)
    {
        Console.WriteLine("Начинается обновление конфигураций в CustomConfigurationProvider...");

        bool hasChanges = _lastConfigurations.Count != newConfigurations.Count ||
                          !_lastConfigurations.All(kv =>
                              newConfigurations.TryGetValue(kv.Key, out string? newValue) && kv.Value == newValue);

        if (hasChanges)
        {
            Console.WriteLine("Изменения в конфигурации обнаружены...");
            _lastConfigurations = new Dictionary<string, string?>(newConfigurations);
            Data = newConfigurations;
            Console.WriteLine("OnReload вызван");
            OnReload();
        }
        else
        {
            Console.WriteLine("Изменений в конфигурации не обнаружено");
        }
    }
}