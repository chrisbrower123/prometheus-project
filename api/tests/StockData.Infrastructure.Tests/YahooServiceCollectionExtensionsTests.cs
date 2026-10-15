using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StockData.Domain.Abstractions;
using StockData.Domain.Exceptions;
using StockData.Infrastructure.Providers.Yahoo;
using StockData.Infrastructure.Tests.TestSupport;

namespace StockData.Infrastructure.Tests;

public sealed class YahooServiceCollectionExtensionsTests
{
    private static IConfiguration Configuration(string baseUrl = "https://query1.finance.yahoo.com") => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        [$"{YahooFinanceOptions.SectionName}:BaseUrl"] = baseUrl,
        [$"{YahooFinanceOptions.SectionName}:UserAgent"] = "test-agent",
        [$"{YahooFinanceOptions.SectionName}:Timeout"] = "00:00:10",
    }).Build();

    [Fact]
    public void AddYahooMarketData_ValidConfig_ResolvesYahooProvider()
    {
        using var provider = new ServiceCollection().AddYahooMarketData(Configuration()).BuildServiceProvider();
        Assert.IsAssignableFrom<IMarketDataProvider>(provider.GetRequiredService<IMarketDataProvider>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-url")]
    public async Task AddYahooMarketData_InvalidConfig_FailsHostStart(string baseUrl)
    {
        using var host = Host.CreateDefaultBuilder().ConfigureServices(services => services.AddYahooMarketData(Configuration(baseUrl))).Build();
        await Assert.ThrowsAsync<OptionsValidationException>(() => host.StartAsync());
    }

    [Fact]
    public async Task AddYahooMarketData_ConfiguredClient_SendsBaseAddressAndUserAgent()
    {
        var handler = new StubHttpMessageHandler((request, _) => Task.FromResult(ResponseFactory.Json(System.Net.HttpStatusCode.OK, Payloads.Valid)));
        using var host = Host.CreateDefaultBuilder().ConfigureServices(services =>
        {
            services.AddYahooMarketData(Configuration());
            services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        }).Build();
        await host.StartAsync();
        await host.Services.GetRequiredService<IMarketDataProvider>().GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, StockData.Domain.Models.IntradayInterval.FifteenMinutes, CancellationToken.None);
        Assert.Contains("query1.finance.yahoo.com", handler.Requests[0].RequestUri!.Host);
    }

    [Fact]
    public async Task Resilience_Transient503ThenSuccess_ReturnsData()
    {
        var calls = 0;
        var handler = new StubHttpMessageHandler((_, _) => { calls++; return Task.FromResult(calls == 1 ? ResponseFactory.Json(System.Net.HttpStatusCode.ServiceUnavailable, "error") : ResponseFactory.Json(System.Net.HttpStatusCode.OK, Payloads.Valid)); });
        using var host = Host.CreateDefaultBuilder().ConfigureServices(services =>
        {
            services.AddYahooMarketData(Configuration(), options => { options.Retry.MaxRetryAttempts = 2; options.Retry.Delay = TimeSpan.Zero; });
            services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        }).Build();
        await host.StartAsync();
        await host.Services.GetRequiredService<IMarketDataProvider>().GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, StockData.Domain.Models.IntradayInterval.FifteenMinutes, CancellationToken.None);
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task Resilience_Persistent503_RetriesTwiceThenThrowsProviderUnavailable()
    {
        var calls = 0;
        var handler = new StubHttpMessageHandler((_, _) => { calls++; return Task.FromResult(ResponseFactory.Json(System.Net.HttpStatusCode.ServiceUnavailable, "error")); });
        using var host = Host.CreateDefaultBuilder().ConfigureServices(services =>
        {
            services.AddYahooMarketData(Configuration(), options => { options.Retry.MaxRetryAttempts = 2; options.Retry.Delay = TimeSpan.Zero; });
            services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        }).Build();
        await host.StartAsync();
        await Assert.ThrowsAsync<ProviderUnavailableException>(() => host.Services.GetRequiredService<IMarketDataProvider>().GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, StockData.Domain.Models.IntradayInterval.FifteenMinutes, CancellationToken.None));
        Assert.Equal(3, calls);
    }

    [Fact]
    public async Task Resilience_429_IsNotRetriedAndSurfacesRetryAfter()
    {
        var calls = 0;
        var handler = new StubHttpMessageHandler((_, _) => { calls++; var response = ResponseFactory.Json(System.Net.HttpStatusCode.TooManyRequests, ""); response.Headers.TryAddWithoutValidation("Retry-After", "30"); return Task.FromResult(response); });
        using var host = Host.CreateDefaultBuilder().ConfigureServices(services =>
        {
            services.AddYahooMarketData(Configuration(), options => { options.Retry.MaxRetryAttempts = 2; options.Retry.Delay = TimeSpan.Zero; });
            services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        }).Build();
        await host.StartAsync();
        var exception = await Assert.ThrowsAsync<ProviderRateLimitedException>(() => host.Services.GetRequiredService<IMarketDataProvider>().GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, StockData.Domain.Models.IntradayInterval.FifteenMinutes, CancellationToken.None));
        Assert.Equal(1, calls);
        Assert.Equal(TimeSpan.FromSeconds(30), exception.RetryAfter);
    }
}
