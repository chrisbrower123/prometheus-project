namespace StockData.Domain.Models;

/// <summary>Intraday bars for a symbol. Days are grouped in the exchange timezone, not UTC.</summary>
public sealed record IntradaySeries
{
    public Symbol Symbol { get; }
    public TimeZoneInfo ExchangeTimeZone { get; }
    public IReadOnlyList<IntradayBar> Bars { get; }

    public IntradaySeries(Symbol symbol, TimeZoneInfo exchangeTimeZone, IReadOnlyList<IntradayBar> bars)
    {
        ArgumentNullException.ThrowIfNull(exchangeTimeZone);
        ArgumentNullException.ThrowIfNull(bars);
        if (bars.Any(bar => bar is null)) throw new ArgumentException("Bars cannot contain null elements.", nameof(bars));

        Symbol = symbol;
        ExchangeTimeZone = exchangeTimeZone;
        Bars = new System.Collections.ObjectModel.ReadOnlyCollection<IntradayBar>(bars.ToArray());
    }
}
