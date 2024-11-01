using Microsoft.Extensions.Options;
using Npgsql;
using Task1;
using Task2;
using Task3;
using Task3.Migrations;
using Task3.Repositories;
using Task3.Services;

namespace WebApp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureCustomServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<CustomConfigurationOptions>(configuration.GetSection("ConfigurationService"));
        services.Configure<DatabaseOptions>(configuration.GetSection("DatabaseOptions"));

        services.AddRefitConfigurationService(configuration);

        services.AddSingleton<NpgsqlDataSource>(provider =>
        {
            DatabaseOptions options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            return NpgsqlDataSource.Create(options.ConnectionString);
        });

        services.AddMigrationServices();

        services.AddRepositories();
        services.AddServices();

        services.AddHostedService<ConfigurationUpdateBackgroundService>();
        services.AddHostedService<MigrationBackgroundService>();

        return services;
    }
}