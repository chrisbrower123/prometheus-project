using StockData.Domain.Models;

namespace StockData.Domain.Tests.TestSupport;

public static class TestData
{
    public static readonly TimeZoneInfo NewYork = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
    public static readonly TimeZoneInfo Tokyo = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo");
    public static readonly TimeZoneInfo Kolkata = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

    public static DateTimeOffset Utc(int year, int month, int day, int hour = 12, int minute = 0, int second = 0) =>
        new(year, month, day, hour, minute, second, TimeSpan.Zero);

    public static IntradayBar Bar(DateTimeOffset timestamp, decimal low, decimal high, long volume = 1_000) =>
        new(timestamp, high, low, volume);

    public static IntradaySeries Series(TimeZoneInfo zone, params IntradayBar[] bars) =>
        new(Symbol.Parse("TSLA"), zone, bars);

    public static IntradaySeries Series(params IntradayBar[] bars) => Series(TimeZoneInfo.Utc, bars);
}
