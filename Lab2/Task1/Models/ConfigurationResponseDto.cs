using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Task1.Models;

public class ConfigurationResponseDto
{
    [JsonPropertyName("items")]
    public Collection<ConfigurationItemDto>? Items { get; init; }

    [JsonPropertyName("pageToken")]
    public ConfigurationPageToken? PageToken { get; set; }
}