using Microsoft.Extensions.Options;
using Task1.Models;

namespace Task1;

public class RefitConfigurationServiceClient : IConfigurationServiceClient
{
    private readonly IRefitConfigurationApi _api;
    private readonly int _pageSize;

    public RefitConfigurationServiceClient(IRefitConfigurationApi api, IOptions<ConfigurationServiceOptions> options)
    {
        _api = api;
        _pageSize = options.Value.PageSize;
    }

    public async Task<IReadOnlyList<ConfigurationItem>> GetAllConfigurationsAsync(CancellationToken cancellationToken)
    {
        var configurations = new List<ConfigurationItem>();
        ConfigurationPageToken? nextPageToken = null;

        do
        {
            ConfigurationResponseDto dto = await _api.GetConfigurationsAsync(_pageSize, nextPageToken?.Id, cancellationToken);

            configurations.AddRange((dto.Items ?? throw new InvalidOperationException()).Select(item => new ConfigurationItem
            {
                Key = new ConfigurationKey(item.Key),
                Value = new ConfigurationValue(item.Value),
            }));

            nextPageToken = dto.PageToken;
        }
        while (nextPageToken != null && nextPageToken != ConfigurationPageToken.Empty);

        return configurations;
    }
}
