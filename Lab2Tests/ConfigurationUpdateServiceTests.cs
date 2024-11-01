using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Task1;
using Task1.Models;
using Task2;
using Xunit;

namespace Lab2Tests;

public class ConfigurationUpdateServiceTests
{
    [Fact]
    public async Task Start_ShouldUpdateConfigurationsPeriodically()
    {
        // Arrange
        var clientMock = new Mock<IConfigurationServiceClient>();
        clientMock
            .Setup(c => c.GetAllConfigurationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConfigurationItem>());
        var providerMock = new Mock<CustomConfigurationProvider>() { CallBase = true };
        providerMock.Setup(p => p.UpdateConfigurations(It.IsAny<Dictionary<string, string?>>()));

        var updateInterval = TimeSpan.FromMilliseconds(100);
        var service = new ConfigurationUpdateService(providerMock.Object, clientMock.Object, updateInterval);

        // Act
        service.Start();

        // Даем сервису время на несколько циклов обновления
        await Task.Delay(350);

        service.Stop();

        // Assert
        clientMock.Verify(c => c.GetAllConfigurationsAsync(It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task Start_ShouldHandleExceptionsAndContinue()
    {
        // Arrange
        int callCount = 0;
        var clientMock = new Mock<IConfigurationServiceClient>();
        clientMock
            .SetupSequence(c => c.GetAllConfigurationsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Temporary error"))
            .ReturnsAsync(new List<ConfigurationItem>());
        clientMock
            .Setup(c => c.GetAllConfigurationsAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callCount++)
            .ReturnsAsync(new List<ConfigurationItem>());

        var provider = new CustomConfigurationProvider();

        var updateInterval = TimeSpan.FromMilliseconds(100);
        var service = new ConfigurationUpdateService(provider, clientMock.Object, updateInterval);

        // Act
        service.Start();

        // Даем сервису время на несколько циклов обновления
        await Task.Delay(500);

        service.Stop();

        // Assert
        callCount.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAllConfigurationsAsync_ShouldReturnConfigurations()
    {
        // Arrange
        var httpClientMock = new Mock<HttpMessageHandler>();
        httpClientMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(
                    @"{
                    ""items"": [
                        { ""key"": ""Key1"", ""value"": ""Value1"" },
                        { ""key"": ""Key2"", ""value"": ""Value2"" }
                    ],
                    ""pageToken"": null
                }"),
            });

        var httpClient = new HttpClient(httpClientMock.Object)
        {
            BaseAddress = new Uri("http://localhost"),
        };

        IOptions<ConfigurationServiceOptions> options = Options.Create(new ConfigurationServiceOptions { PageSize = 100 });
        var client = new HttpClientConfigurationServiceClient(httpClient, options);

        // Act
        IReadOnlyList<ConfigurationItem> configurations = await client.GetAllConfigurationsAsync(CancellationToken.None);

        // Assert
        configurations.Should().HaveCount(2);
        configurations.Should().Contain(c => c.Key.Value == "Key1" && c.Value.Value == "Value1");
        configurations.Should().Contain(c => c.Key.Value == "Key2" && c.Value.Value == "Value2");
    }
}