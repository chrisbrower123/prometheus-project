using StockData.Domain.Abstractions;
using StockData.Domain.Models;
namespace StockData.Domain.Aggregation;
/// <summary>Stub for daily aggregation.</summary>
public sealed class DailyAggregator : IDailyAggregator
{
    /// <inheritdoc />
    public IReadOnlyList<DailySummary> Aggregate(IntradaySeries series)
    {
        // TODO: convert timestamps to exchange local dates, group bars, average highs/lows, sum volume, and order ascending.
        throw new NotImplementedException();
    }
}
