using StockData.Domain.Models;
using StockData.Domain.Tests.TestSupport;
namespace StockData.Domain.Tests;
public sealed class IntradayBarTests
{
    [Fact]
    public void Constructor_ValidBar_StoresValues()
    {
        var timestamp = TestData.Utc(2026, 1, 15);
        var bar = new IntradayBar(timestamp, 110m, 100m, 500);
        Assert.Equal(timestamp, bar.Timestamp); Assert.Equal(110m, bar.High); Assert.Equal(100m, bar.Low); Assert.Equal(500, bar.Volume);
    }
    [Fact]
    public void Constructor_HighBelowLow_ThrowsArgumentException() => Assert.Throws<ArgumentException>(() => new IntradayBar(TestData.Utc(2026, 1, 15), 9m, 10m, 1));
    [Fact]
    public void Constructor_NegativeVolume_ThrowsArgumentOutOfRangeException() => Assert.Throws<ArgumentOutOfRangeException>(() => new IntradayBar(TestData.Utc(2026, 1, 15), 10m, 1m, -1));
    [Fact]
    public void WithExpression_InvalidValue_ThrowsArgumentException()
    {
        var bar = new IntradayBar(TestData.Utc(2026, 1, 15), 10m, 5m, 1);
        Assert.Throws<ArgumentException>(() => bar with { Low = 11m });
    }
}
public sealed class IntradaySeriesTests
{
    private static readonly Symbol Symbol = new("TSLA");
    [Fact]
    public void Constructor_ValidSeries_RetainsIdentity()
    {
        var series = new IntradaySeries(Symbol, TestData.NewYork, []);
        Assert.Equal(Symbol, series.Symbol); Assert.Same(TestData.NewYork, series.ExchangeTimeZone);
    }
    [Fact]
    public void Constructor_NullTimeZone_ThrowsArgumentNullException() => Assert.Throws<ArgumentNullException>(() => new IntradaySeries(Symbol, null!, []));
    [Fact]
    public void Constructor_NullBars_ThrowsArgumentNullException() => Assert.Throws<ArgumentNullException>(() => new IntradaySeries(Symbol, TimeZoneInfo.Utc, null!));
    [Fact]
    public void Constructor_CopiesInputBars()
    {
        var input = new List<IntradayBar> { TestData.Bar(TestData.Utc(2026, 1, 15), 100m, 110m) };
        var series = new IntradaySeries(Symbol, TimeZoneInfo.Utc, input);
        input.Clear();
        Assert.Single(series.Bars);
    }
}
public sealed class DailySummaryTests
{
    [Fact]
    public void Constructor_PreservesPrecisionAndLongVolume()
    {
        var summary = new DailySummary(new DateOnly(2026, 1, 15), 40.29583333333333333333333333m, 50m, 5_000_000_000L);
        Assert.Equal(40.29583333333333333333333333m, summary.LowAverage); Assert.Equal(5_000_000_000L, summary.Volume);
    }
}
public sealed class IntradayIntervalTests
{
    [Fact]
    public void EnumValues_ContainsOnlyFifteenMinutes() => Assert.Equal([IntradayInterval.FifteenMinutes], Enum.GetValues<IntradayInterval>());
}
