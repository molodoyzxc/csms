using Microsoft.Extensions.Options;
using System.Text.Json;
using Task1.Models;

namespace Task1;

public class HttpClientConfigurationServiceClient : IConfigurationServiceClient
{
    private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;
    private readonly int _pageSize;

    public HttpClientConfigurationServiceClient(HttpClient httpClient, IOptions<ConfigurationServiceOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _pageSize = options.Value.PageSize;
    }

    public async Task<IReadOnlyList<ConfigurationItem>> GetAllConfigurationsAsync(CancellationToken cancellationToken)
    {
        var configurations = new List<ConfigurationItem>();
        ConfigurationPageToken? nextPageToken = null;

        do
        {
            string requestUri = $"configurations?pageSize={_pageSize}";
            if (nextPageToken != null && nextPageToken != ConfigurationPageToken.Empty)
            {
                requestUri += $"&pageToken={nextPageToken.Id}";
            }

            HttpResponseMessage response = await _httpClient.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            ConfigurationResponseDto? dto = JsonSerializer.Deserialize<ConfigurationResponseDto>(content, _serializerOptions);

            if (dto != null)
            {
                configurations.AddRange((dto.Items ?? throw new InvalidOperationException()).Select(item => new ConfigurationItem
                {
                    Key = new ConfigurationKey(item.Key),
                    Value = new ConfigurationValue(item.Value),
                }));

                nextPageToken = dto.PageToken;
            }
        }
        while (nextPageToken != null && nextPageToken != ConfigurationPageToken.Empty);

        return configurations;
    }
}