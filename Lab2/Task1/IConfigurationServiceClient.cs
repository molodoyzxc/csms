using Task1.Models;

namespace Task1;

public interface IConfigurationServiceClient
{
    Task<IReadOnlyList<ConfigurationItem>> GetAllConfigurationsAsync(CancellationToken cancellationToken);
}
