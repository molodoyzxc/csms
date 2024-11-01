using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Task3.Migrations;

public static class MigrationExtensions
{
    public static IServiceCollection AddMigrationServices(this IServiceCollection services)
    {
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb =>
            {
                rb.AddPostgres()
                    .WithGlobalConnectionString(services.BuildServiceProvider()
                        .GetRequiredService<IOptions<DatabaseOptions>>()
                        .Value.ConnectionString)
                    .ScanIn(typeof(InitialCreateMigration).Assembly)
                    .For.Migrations();
            })
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        return services;
    }

    public static void ApplyMigrations(this IServiceProvider serviceProvider)
    {
        IMigrationRunner runner = serviceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
}