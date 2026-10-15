using StockData.Domain.Abstractions;
using StockData.Domain.Models;
namespace StockData.Domain.Aggregation;
/// <summary>Stub for daily aggregation.</summary>
public sealed class DailyAggregator : IDailyAggregator
{
    /// <inheritdoc />
    public IReadOnlyList<DailySummary> Aggregate(IntradaySeries series)
    {
        ArgumentNullException.ThrowIfNull(series);

        var latestByInstant = new Dictionary<long, IntradayBar>();
        foreach (var bar in series.Bars)
        {
            latestByInstant[bar.Timestamp.UtcTicks] = bar;
        }

        var summaries = latestByInstant.Values
            .GroupBy(bar => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(bar.Timestamp, series.ExchangeTimeZone).DateTime))
            .OrderBy(group => group.Key)
            .Select(group => new DailySummary(
                group.Key,
                group.Average(bar => bar.Low),
                group.Average(bar => bar.High),
                group.Sum(bar => bar.Volume)))
            .ToArray();

        return new System.Collections.ObjectModel.ReadOnlyCollection<DailySummary>(summaries);
    }
}
