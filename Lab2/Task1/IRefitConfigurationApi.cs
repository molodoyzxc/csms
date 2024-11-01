using Refit;
using Task1.Models;

namespace Task1;

public interface IRefitConfigurationApi
{
    [Get("/configurations")]
    Task<ConfigurationResponseDto> GetConfigurationsAsync(
        [Query] int pageSize,
        [Query] long? pageToken,
        CancellationToken cancellationToken);
}