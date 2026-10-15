using StockData.Domain.Abstractions;
using StockData.Domain.Exceptions;
using StockData.Domain.Models;
namespace StockData.Domain.Services;
/// <summary>Application service for retrieving and aggregating daily summaries.</summary>
public sealed class DailySummaryService : IDailySummaryService
{
    private readonly IMarketDataProvider provider;
    private readonly IDailyAggregator aggregator;
    private readonly TimeProvider timeProvider;

    public DailySummaryService(IMarketDataProvider provider, IDailyAggregator aggregator, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(aggregator);
        ArgumentNullException.ThrowIfNull(timeProvider);
        this.provider = provider;
        this.aggregator = aggregator;
        this.timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DailySummary>> GetDailySummariesAsync(Symbol symbol, CancellationToken cancellationToken)
    {
        if (symbol.Value is null) throw new InvalidSymbolException(null);
        cancellationToken.ThrowIfCancellationRequested();

        var to = timeProvider.GetUtcNow();
        var from = to.AddMonths(-1);
        var series = await provider.GetIntradaySeriesAsync(symbol, from, to, IntradayInterval.FifteenMinutes, cancellationToken);
        if (series.Bars.Count == 0) throw new NoDataAvailableException(symbol);
        return aggregator.Aggregate(series);
    }
}
