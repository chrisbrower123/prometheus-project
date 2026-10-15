using StockData.Domain.Aggregation;
using StockData.Domain.Tests.TestSupport;
namespace StockData.Domain.Tests;
public sealed class DailyAggregatorTests
{
    private readonly DailyAggregator _aggregator = new();
    [Fact]
    public void Aggregate_SingleBar_ReturnsItsStatistics()
    {
        var summary = Assert.Single(_aggregator.Aggregate(TestData.Series(TestData.Bar(TestData.Utc(2026, 1, 15), 10m, 15m, 100))));
        Assert.Equal(10m, summary.LowAverage); Assert.Equal(15m, summary.HighAverage); Assert.Equal(100, summary.Volume);
    }
    [Fact]
    public void Aggregate_MultipleBars_UsesMeansAndLongVolume()
    {
        var summary = Assert.Single(_aggregator.Aggregate(TestData.Series(
            TestData.Bar(TestData.Utc(2026, 1, 15, 12), 10m, 15m, int.MaxValue),
            TestData.Bar(TestData.Utc(2026, 1, 15, 13), 20m, 25m, int.MaxValue),
            TestData.Bar(TestData.Utc(2026, 1, 15, 14), 60m, 65m, int.MaxValue))));
        Assert.Equal(30m, summary.LowAverage); Assert.Equal(35m, summary.HighAverage); Assert.Equal(6_442_450_941L, summary.Volume);
    }
    [Fact]
    public void Aggregate_TwoDays_ReturnsAscendingSummariesWithoutGapFilling()
    {
        var result = _aggregator.Aggregate(TestData.Series(TestData.Bar(TestData.Utc(2026, 1, 16), 10m, 15m), TestData.Bar(TestData.Utc(2026, 1, 19), 20m, 25m)));
        Assert.Equal([new DateOnly(2026, 1, 16), new DateOnly(2026, 1, 19)], result.Select(summary => summary.Day));
    }
    [Fact]
    public void Aggregate_DuplicateTimestamp_KeepsLastOccurrence()
    {
        var timestamp = TestData.Utc(2026, 1, 15, 12);
        var summary = Assert.Single(_aggregator.Aggregate(TestData.Series(TestData.Bar(timestamp, 10m, 15m, 100), TestData.Bar(timestamp, 20m, 25m, 250))));
        Assert.Equal(20m, summary.LowAverage); Assert.Equal(25m, summary.HighAverage); Assert.Equal(250, summary.Volume);
    }
    [Fact]
    public void Aggregate_EmptySeries_ReturnsEmptyList() => Assert.Empty(_aggregator.Aggregate(TestData.Series()));
    [Fact]
    public void Aggregate_NullSeries_ThrowsArgumentNullException() => Assert.Throws<ArgumentNullException>(() => _aggregator.Aggregate(null!));
    [Fact]
    public void Aggregate_CalledTwice_ReturnsEquivalentResults()
    {
        var series = TestData.Series(TestData.Bar(TestData.Utc(2026, 1, 15), 10m, 15m));
        Assert.Equal(_aggregator.Aggregate(series), _aggregator.Aggregate(series));
    }
}
