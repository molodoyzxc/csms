using Microsoft.Extensions.DependencyInjection;

namespace Task3.Repositories;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<ProductRepository>();
        services.AddTransient<OrderRepository>();
        services.AddTransient<OrderItemRepository>();
        services.AddTransient<OrderHistoryRepository>();
        return services;
    }
}
