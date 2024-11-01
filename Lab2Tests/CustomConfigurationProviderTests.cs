using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Task1;
using Task1.Models;
using Task2;
using Xunit;

namespace Lab2Tests;

public class CustomConfigurationProviderTests
{
    [Fact]
    public void Load_ShouldLoadInitialConfigurations()
    {
        // Arrange
        var configurations = new List<ConfigurationItem>
        {
            new ConfigurationItem(1, new ConfigurationKey("Key1"), new ConfigurationValue("Value1")),
            new ConfigurationItem(2, new ConfigurationKey("Key2"), new ConfigurationValue("Value2")),
        };

        var provider = new CustomConfigurationProvider();

        // Преобразуем список конфигураций в словарь
        var configDictionary = new Dictionary<string, string?>();
        foreach (ConfigurationItem configuration in configurations)
            configDictionary.Add(configuration.Key.Value, configuration.Value.Value);

        // Act
        // Вызываем метод UpdateConfigurations напрямую
        provider.UpdateConfigurations(configDictionary);

        // Assert
        provider.TryGet("Key1", out string? value1).Should().BeTrue();
        value1.Should().Be("Value1");

        provider.TryGet("Key2", out string? value2).Should().BeTrue();
        value2.Should().Be("Value2");
    }

    [Fact]
    public void UpdateConfigurations_ShouldUpdateDataAndRaiseOnReload()
    {
        // Arrange
        var initialConfigurations = new Dictionary<string, string?>
        {
            { "Key1", "Value1" },
        };

        var newConfigurations = new Dictionary<string, string?>
        {
            { "Key1", "NewValue1" },
            { "Key2", "Value2" },
        };

        var clientMock = new Mock<IConfigurationServiceClient>();
        var services = new ServiceCollection();
        services.AddSingleton<IConfigurationServiceClient>(clientMock.Object);
        services.AddSingleton<CustomConfigurationProvider>();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        CustomConfigurationProvider provider = serviceProvider.GetRequiredService<CustomConfigurationProvider>();
        provider.UpdateConfigurations(initialConfigurations);

        bool reloadCalled = false;
        provider.GetReloadToken().RegisterChangeCallback(_ => reloadCalled = true, null);

        // Act
        provider.UpdateConfigurations(newConfigurations);

        // Assert
        provider.TryGet("Key1", out string? value1).Should().BeTrue();
        value1.Should().Be("NewValue1");

        provider.TryGet("Key2", out string? value2).Should().BeTrue();
        value2.Should().Be("Value2");

        reloadCalled.Should().BeTrue();
    }

    [Fact]
    public void Load_ShouldHandleExceptionsGracefully()
    {
        // Arrange
        var clientMock = new Mock<IConfigurationServiceClient>();
        clientMock
            .Setup(c => c.GetAllConfigurationsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Exception("Service unavailable"));

        var services = new ServiceCollection();
        services.AddSingleton<IConfigurationServiceClient>(clientMock.Object);
        services.AddSingleton<CustomConfigurationProvider>();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        CustomConfigurationProvider provider = serviceProvider.GetRequiredService<CustomConfigurationProvider>();

        // Act & Assert
        Exception exception = Record.Exception(() => provider.Load());

        // Проверяем, что исключение не было выброшено
        exception.Should().BeNull("потому что метод Load должен обрабатывать исключения самостоятельно");
    }
}