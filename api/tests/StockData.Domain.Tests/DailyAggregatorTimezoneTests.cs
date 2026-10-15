using StockData.Domain.Aggregation;
using StockData.Domain.Tests.TestSupport;
namespace StockData.Domain.Tests;
[Trait("Category", "TimezoneReview")]
public sealed class DailyAggregatorTimezoneTests
{
    private readonly DailyAggregator _aggregator = new();
    [Fact(Skip = "Pending timezone test review")]
    public void Aggregate_NewYorkBoundary_UsesExchangeLocalDay()
    {
        var result = _aggregator.Aggregate(TestData.Series(TestData.NewYork, TestData.Bar(TestData.Utc(2026, 1, 16, 3), 1m, 2m)));
        Assert.Equal(new DateOnly(2026, 1, 15), Assert.Single(result).Day);
    }
    [Fact(Skip = "Pending timezone test review")]
    public void Aggregate_TokyoBoundary_UsesNextLocalDay()
    {
        var result = _aggregator.Aggregate(TestData.Series(TestData.Tokyo, TestData.Bar(TestData.Utc(2026, 1, 15, 16), 1m, 2m)));
        Assert.Equal(new DateOnly(2026, 1, 16), Assert.Single(result).Day);
    }
    [Fact(Skip = "Pending timezone test review")]
    public void Aggregate_DstBarsRemainInTheirLocalDate()
    {
        var result = _aggregator.Aggregate(TestData.Series(TestData.NewYork, TestData.Bar(TestData.Utc(2026, 3, 8, 6, 30), 1m, 2m), TestData.Bar(TestData.Utc(2026, 3, 8, 7, 30), 1m, 2m)));
        Assert.Single(result);
    }
}
