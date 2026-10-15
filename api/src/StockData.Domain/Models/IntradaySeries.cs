namespace StockData.Domain.Models;

/// <summary>Intraday bars for a symbol. Days are grouped in the exchange timezone, not UTC.</summary>
public sealed record IntradaySeries(Symbol Symbol, TimeZoneInfo ExchangeTimeZone, IReadOnlyList<IntradayBar> Bars)
{
    // TODO: reject null timezone/bars/elements and defensively copy Bars.
}
