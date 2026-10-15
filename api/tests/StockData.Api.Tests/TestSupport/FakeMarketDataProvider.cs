using StockData.Domain.Abstractions;
using StockData.Domain.Models;

namespace StockData.Api.Tests.TestSupport;

public sealed class FakeMarketDataProvider : IMarketDataProvider
{
    public List<(Symbol Symbol, DateTimeOffset From, DateTimeOffset To, IntradayInterval Interval, CancellationToken CancellationToken)> Calls { get; } = [];
    private IntradaySeries? result;
    private Exception? failure;

    public FakeMarketDataProvider Returns(IntradaySeries value) { result = value; failure = null; return this; }
    public FakeMarketDataProvider Throws(Exception value) { failure = value; result = null; return this; }

    public Task<IntradaySeries> GetIntradaySeriesAsync(Symbol symbol, DateTimeOffset from, DateTimeOffset until, IntradayInterval interval, CancellationToken cancellationToken)
    {
        Calls.Add((symbol, from, until, interval, cancellationToken));
        if (failure is not null) throw failure;
        return Task.FromResult(result ?? new IntradaySeries(symbol, TimeZoneInfo.Utc, Array.Empty<IntradayBar>()));
    }
}
