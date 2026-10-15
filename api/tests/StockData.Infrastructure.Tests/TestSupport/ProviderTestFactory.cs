using Microsoft.Extensions.Options;
using StockData.Domain.Models;
using StockData.Infrastructure.Providers.Yahoo;

namespace StockData.Infrastructure.Tests.TestSupport;

public static class ProviderTestFactory
{
    internal static YahooMarketDataProvider Create(StubHttpMessageHandler handler, YahooFinanceOptions? options = null, TimeProvider? time = null)
    {
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://query1.finance.yahoo.com") };
        return new YahooMarketDataProvider(client, Options.Create(options ?? new YahooFinanceOptions { BaseUrl = "https://query1.finance.yahoo.com" }), time ?? TimeProvider.System);
    }

    public static Symbol Symbol => new("TSLA");
    public static DateTimeOffset From => new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public static DateTimeOffset To => From.AddHours(1);
}
