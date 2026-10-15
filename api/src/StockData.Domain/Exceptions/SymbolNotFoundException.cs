using StockData.Domain.Models;
namespace StockData.Domain.Exceptions;
/// <summary>Raised when a symbol is not found.</summary>
public sealed class SymbolNotFoundException(Symbol symbol) : StockDataException($"Symbol not found: {symbol}")
{
    /// <summary>The missing symbol.</summary>
    public Symbol Symbol { get; } = symbol;
}
