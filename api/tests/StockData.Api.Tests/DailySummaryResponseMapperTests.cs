using System.Globalization;
using StockData.Api.Contracts;
using StockData.Api.Tests.TestSupport;
using StockData.Domain.Models;

namespace StockData.Api.Tests;

[Collection("Culture")]
public sealed class DailySummaryResponseMapperTests
{
    [Theory]
    [InlineData(40.29585, 40.2959)]
    [InlineData(40.29584999, 40.2958)]
    [InlineData(0.00005, 0.0001)]
    [InlineData(0.00004, 0)]
    [InlineData(40.29583333333333333333333333, 40.2958)]
    public void ToResponse_Averages_RoundToFourDecimalsAwayFromZero(decimal value, decimal expected)
    {
        var response = new DailySummary(new DateOnly(2026, 1, 15), value, value, 1).ToResponse();
        Assert.Equal(expected, response.LowAverage);
        Assert.Equal(expected, response.HighAverage);
    }

    [Theory]
    [InlineData(40.29)]
    [InlineData(40.2958)]
    [InlineData(100)]
    [InlineData(0)]
    public void ToResponse_ValuesWithFourOrFewerDecimals_AreUnchanged(decimal value)
    {
        var response = new DailySummary(new DateOnly(2026, 1, 15), value, value, 1).ToResponse();
        Assert.Equal(value, response.LowAverage);
        Assert.Equal(value, response.HighAverage);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("th-TH")]
    [InlineData("ar-SA")]
    public void ToResponse_Day_FormatsInvariantlyUnderAnyCulture(string cultureName)
    {
        var original = CultureInfo.CurrentCulture;
        var originalUi = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
            Assert.Equal("2026-01-15", new DailySummary(new DateOnly(2026, 1, 15), 1, 2, 1).ToResponse().Day);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
            CultureInfo.CurrentUICulture = originalUi;
        }
    }

    [Fact]
    public void ToResponse_LargeVolume_PassesThroughUnchanged() => Assert.Equal(5_000_000_000L, new DailySummary(new DateOnly(2026, 1, 15), 1, 2, 5_000_000_000L).ToResponse().Volume);

    [Fact]
    public void ToResponse_Collection_PreservesCountOrderAndAllFields()
    {
        var summaries = new[]
        {
            new DailySummary(new DateOnly(2026, 1, 15), 1, 2, 3),
            new DailySummary(new DateOnly(2026, 1, 16), 4, 5, 6),
            new DailySummary(new DateOnly(2026, 1, 17), 7, 8, 9),
        };
        var responses = summaries.ToResponse();
        Assert.Equal(3, responses.Count);
        Assert.Equal("2026-01-15", responses[0].Day); Assert.Equal(3, responses[2].Volume);
    }
}
