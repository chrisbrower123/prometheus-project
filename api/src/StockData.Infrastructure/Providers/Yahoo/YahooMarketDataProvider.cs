using Microsoft.Extensions.Options;
using StockData.Domain.Abstractions;
using StockData.Domain.Models;
namespace StockData.Infrastructure.Providers.Yahoo;
internal sealed class YahooMarketDataProvider(HttpClient httpClient, IOptions<YahooFinanceOptions> options) : IMarketDataProvider
{
    public Task<IntradaySeries> GetIntradaySeriesAsync(Symbol symbol, DateTimeOffset from, DateTimeOffset until, IntradayInterval interval, CancellationToken cancellationToken)
    {
        _ = httpClient;
        _ = options;
        _ = symbol;
        _ = from;
        _ = until;
        _ = interval;
        _ = cancellationToken;
        // TODO: build the encoded Yahoo request, send it, translate status/transport/JSON failures, and map the response.
        throw new NotImplementedException();
    }
}
