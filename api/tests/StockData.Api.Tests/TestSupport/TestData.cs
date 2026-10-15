using StockData.Domain.Models;

namespace StockData.Api.Tests.TestSupport;

public static class TestData
{
    public static readonly TimeZoneInfo NewYork = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
    public static DateTimeOffset Utc(int year, int month, int day, int hour = 12) => new(year, month, day, hour, 0, 0, TimeSpan.Zero);
    public static IntradayBar Bar(DateTimeOffset timestamp, decimal low, decimal high, long volume = 1000) => new(timestamp, high, low, volume);
    public static IntradaySeries Series(TimeZoneInfo zone, params IntradayBar[] bars) => new(new Symbol("TSLA"), zone, bars);
}
