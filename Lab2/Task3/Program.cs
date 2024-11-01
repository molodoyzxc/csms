using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Text.Json;
using Task1;
using Task2;
using Task3.Migrations;
using Task3.Models;
using Task3.Repositories;
using Task3.Services;

namespace Task3;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();

        ConfigureServices(services);

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        serviceProvider.ApplyMigrations();
        IConfigurationProvider? configProvider = serviceProvider.GetService<IConfigurationProvider>();
        Console.WriteLine(configProvider?.GetType().Name);

        ProductService productService = serviceProvider.GetRequiredService<ProductService>();
        OrderService orderService = serviceProvider.GetRequiredService<OrderService>();

        await productService.CreateProductAsync("Product1", 100);
        await productService.CreateProductAsync("Product2", 200);
        await productService.CreateProductAsync("Product3", 300);

        IEnumerable<Product> products = await productService.GetProductsAsync(1, 100);
        var productList = products.ToList();

        long orderId = await orderService.CreateOrderAsync("user1");

        await orderService.AddItemToOrderAsync(
            orderId,
            productList[0].ProductId,
            2);
        await orderService.AddItemToOrderAsync(
            orderId,
            productList[1].ProductId,
            1);

        IEnumerable<OrderItem> orderItems = await orderService.GetOrderItemsAsync(orderId);
        OrderItem? itemToRemove = orderItems.FirstOrDefault();
        if (itemToRemove != null)
        {
            await orderService.RemoveItemFromOrderAsync(orderId, itemToRemove.OrderItemId);
        }

        await orderService.ChangeOrderStatusAsync(orderId, "processing");

        await orderService.ChangeOrderStatusAsync(orderId, "completed");

        IEnumerable<OrderHistory> orderHistory = (await orderService.GetOrderHistoryAsync(orderId))
            .Select(item => new OrderHistory
            {
                OrderHistoryItemId = item.OrderHistoryItemId,
                OrderId = item.OrderId,
                OrderHistoryItemCreatedAt = item.OrderHistoryItemCreatedAt,
                OrderHistoryItemKind = item.OrderHistoryItemKind,
                OrderHistoryItemPayload = JsonSerializer.Serialize(item),
            });

        Console.WriteLine("История заказа:");
        foreach (OrderHistory history in orderHistory)
        {
            Console.WriteLine($"Тип: {history.OrderHistoryItemKind}, Данные: {history.OrderHistoryItemPayload}");
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        services.Configure<CustomConfigurationOptions>(configuration.GetSection("ConfigurationService"));

        services.AddRefitConfigurationService(configuration);

        services.AddCustomConfigurationProvider();

        ServiceProvider tempServiceProvider = services.BuildServiceProvider();

        IConfigurationRoot finalConfiguration = new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .AddCustomConfiguration(tempServiceProvider)
            .Build();

        services.AddSingleton<IConfiguration>(finalConfiguration);

        services.Configure<DatabaseOptions>(finalConfiguration.GetSection("DatabaseOptions"));

        services.AddSingleton<NpgsqlDataSource>(provider =>
        {
            DatabaseOptions options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            return NpgsqlDataSource.Create(options.ConnectionString);
        });

        services.AddMigrationServices();
        services.AddRepositories();
        services.AddServices();
    }
}