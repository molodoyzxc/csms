using Microsoft.Extensions.DependencyInjection;

namespace Task3.Services;

public static class ServiceExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddTransient<ProductService>();
        services.AddTransient<OrderService>();
        return services;
    }
}
