using StockData.Domain.Models;
namespace StockData.Domain.Abstractions;
/// <summary>Provides intraday market data and translates provider failures into domain exceptions.</summary>
public interface IMarketDataProvider
{
    /// <summary>Gets a series, skipping null or incomplete bars.</summary>
    Task<IntradaySeries> GetIntradaySeriesAsync(Symbol symbol, DateTimeOffset from, DateTimeOffset until, IntradayInterval interval, CancellationToken cancellationToken);
}
