using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;

namespace Task1;

public static class ConfigurationServiceExtensions
{
    public static IServiceCollection AddHttpClientConfigurationService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ConfigurationServiceOptions>(configuration.GetSection("ConfigurationService"));

        services.AddHttpClient<HttpClientConfigurationServiceClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                ConfigurationServiceOptions options =
                    sp.GetRequiredService<IOptions<ConfigurationServiceOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseAddress);
            });

        services.AddScoped<IConfigurationServiceClient, HttpClientConfigurationServiceClient>();
        return services;
    }

    public static IServiceCollection AddRefitConfigurationService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ConfigurationServiceOptions>(configuration.GetSection("ConfigurationService"));

        services.AddRefitClient<IRefitConfigurationApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                ConfigurationServiceOptions options =
                    sp.GetRequiredService<IOptions<ConfigurationServiceOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseAddress);
            });

        services.AddScoped<IConfigurationServiceClient, RefitConfigurationServiceClient>();
        return services;
    }
}