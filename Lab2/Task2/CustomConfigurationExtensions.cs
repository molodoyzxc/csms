using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Task1;

namespace Task2;

public static class CustomConfigurationExtensions
{
    public static IConfigurationBuilder AddCustomConfiguration(
        this IConfigurationBuilder builder,
        IServiceProvider serviceProvider)
    {
        var source = new CustomConfigurationSource(serviceProvider);
        builder.Add(source);
        return builder;
    }

    public static IServiceCollection AddConfigurationUpdateService(
        this IServiceCollection services,
        IConfigurationServiceClient client,
        TimeSpan updateInterval)
    {
        services.AddSingleton<IConfigurationProvider>(sp =>
        {
            var provider = new CustomConfigurationProvider();
            var updateService = new ConfigurationUpdateService(provider, client, updateInterval);
            updateService.Start();
            return provider;
        });
        return services;
    }

    public static IServiceCollection AddCustomConfigurationProvider(
        this IServiceCollection services)
    {
        services.AddSingleton<CustomConfigurationProvider>();
        services.AddSingleton<IConfigurationProvider>(sp => sp.GetRequiredService<CustomConfigurationProvider>());
        return services;
    }
}