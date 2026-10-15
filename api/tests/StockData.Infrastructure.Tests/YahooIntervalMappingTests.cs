using StockData.Domain.Models;
using StockData.Infrastructure.Providers.Yahoo;
using StockData.Infrastructure.Tests.TestSupport;

namespace StockData.Infrastructure.Tests;

public sealed class YahooIntervalMappingTests
{
    [Fact]
    public async Task YahooIntervalMapping_CoversEveryIntradayIntervalValue()
    {
        var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(ResponseFactory.Json(System.Net.HttpStatusCode.OK, Payloads.Valid)));
        var provider = ProviderTestFactory.Create(handler);
        await provider.GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, IntradayInterval.FifteenMinutes, CancellationToken.None);
    }
}
