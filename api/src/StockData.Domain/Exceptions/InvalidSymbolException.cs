using StockData.Domain.Models;
namespace StockData.Domain.Exceptions;
/// <summary>Raised when a symbol is invalid.</summary>
public sealed class InvalidSymbolException(string? input) : StockDataException($"Invalid symbol: {input}")
{
    /// <summary>The rejected input.</summary>
    public string? Input { get; } = input;
}
