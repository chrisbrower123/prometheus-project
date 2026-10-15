using StockData.Domain.Models;
namespace StockData.Domain.Exceptions;
/// <summary>Raised when no data is available.</summary>
public sealed class NoDataAvailableException(Symbol symbol) : StockDataException($"No data available: {symbol}")
{
    /// <summary>The symbol.</summary>
    public Symbol Symbol { get; } = symbol;
}
