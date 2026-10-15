namespace StockData.Domain.Models;

/// <summary>An immutable intraday market bar.</summary>
public sealed record IntradayBar(DateTimeOffset Timestamp, decimal High, decimal Low, long Volume);
