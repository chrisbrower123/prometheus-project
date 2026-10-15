namespace StockData.Domain.Models;

/// <summary>An immutable intraday market bar.</summary>
public sealed record IntradayBar(DateTimeOffset Timestamp, decimal High, decimal Low, long Volume)
{
    // TODO: validate High >= Low, Low >= 0, and Volume >= 0 in property init accessors so with-expressions are covered.
}
