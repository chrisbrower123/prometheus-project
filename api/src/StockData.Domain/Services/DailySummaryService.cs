using StockData.Domain.Abstractions;
using StockData.Domain.Models;
namespace StockData.Domain.Services;
/// <summary>Stub for the daily summary application service.</summary>
// TODO: add ArgumentNullException guards for provider, aggregator, and timeProvider.
public sealed class DailySummaryService(IMarketDataProvider provider, IDailyAggregator aggregator, TimeProvider timeProvider) : IDailySummaryService
{
    /// <inheritdoc />
    public Task<IReadOnlyList<DailySummary>> GetDailySummariesAsync(Symbol symbol, CancellationToken cancellationToken)
    {
        _ = provider;
        _ = aggregator;
        _ = timeProvider;
        _ = symbol;
        _ = cancellationToken;
        // TODO: compute the last-month window, retrieve data, reject empty bars, and aggregate it.
        throw new NotImplementedException();
    }
}
