namespace StockData.Domain.Models;

/// <summary>A normalized market symbol.</summary>
public readonly record struct Symbol(string Value)
{
    /// <summary>Parses and normalizes a symbol.</summary>
    public static Symbol Parse(string? input)
    {
        // TODO: trim, uppercase, validate allowed characters and length, and throw InvalidSymbolException when invalid.
        throw new NotImplementedException();
    }

    /// <summary>Attempts to parse a symbol.</summary>
    public static bool TryParse(string? input, out Symbol symbol)
    {
        symbol = default;
        // TODO: validate and normalize the input without throwing for invalid symbols.
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}
