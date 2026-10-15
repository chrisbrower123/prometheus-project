using StockData.Domain.Models;
namespace StockData.Domain.Abstractions;
/// <summary>Aggregates bars by exchange-local day.</summary>
public interface IDailyAggregator
{
    /// <summary>Computes mean lows, mean highs, and summed volume ordered by day.</summary>
    IReadOnlyList<DailySummary> Aggregate(IntradaySeries series);
}
