using StockData.Domain.Exceptions;

namespace StockData.Domain.Models;

/// <summary>A normalized market symbol.</summary>
public readonly record struct Symbol
{
    public string? Value { get; private init; }

    public Symbol(string? value) => Value = value;

    /// <summary>Parses and normalizes a symbol.</summary>
    public static Symbol Parse(string? input)
    {
        if (!TryNormalize(input, out var normalized))
        {
            throw new InvalidSymbolException(input);
        }

        return new Symbol(normalized);
    }

    /// <summary>Attempts to parse a symbol.</summary>
    public static bool TryParse(string? input, out Symbol symbol)
    {
        if (TryNormalize(input, out var normalized))
        {
            symbol = new Symbol(normalized);
            return true;
        }

        symbol = default;
        return false;
    }

    private static bool TryNormalize(string? input, out string normalized)
    {
        normalized = string.Empty;
        if (input is null) return false;

        var trimmed = input.Trim();
        if (trimmed.Length is < 1 or > 12 || trimmed.EndsWith('.') || trimmed.EndsWith('-')) return false;

        var hasAlphaNumeric = false;
        foreach (var character in trimmed)
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                hasAlphaNumeric = true;
                continue;
            }

            if (character is not ('.' or '-' or '^' or '=')) return false;
        }

        if (!hasAlphaNumeric) return false;
        normalized = trimmed.ToUpperInvariant();
        return true;
    }

    /// <inheritdoc />
    public override string ToString() => Value!;
}
