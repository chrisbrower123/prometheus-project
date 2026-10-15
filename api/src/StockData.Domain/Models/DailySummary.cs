namespace StockData.Domain.Models;

/// <summary>A daily aggregate. Full precision is retained; rounding to four decimals is an API concern.</summary>
public sealed record DailySummary(DateOnly Day, decimal LowAverage, decimal HighAverage, long Volume);
