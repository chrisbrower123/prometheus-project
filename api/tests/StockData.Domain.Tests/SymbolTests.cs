using StockData.Domain.Exceptions;
using StockData.Domain.Models;
namespace StockData.Domain.Tests;
public sealed class SymbolTests
{
    public static TheoryData<string, string> NormalizationCases => new() { { "tsla", "TSLA" }, { "  BrK.b ", "BRK.B" } };
    public static TheoryData<string> InvalidCases => new() { "", "TS LA", "TSLA?interval=1d", "TSLA.", ".", "TS\0LA" };
    [Theory, MemberData(nameof(NormalizationCases))]
    public void Parse_NormalizesInput(string input, string expected) => Assert.Equal(expected, Symbol.Parse(input).Value);
    [Theory]
    [InlineData("TSLA")]
    [InlineData("^GSPC")]
    [InlineData("EURUSD=X")]
    public void Parse_AcceptsValidFormats(string input) => Assert.Equal(input.ToUpperInvariant(), Symbol.Parse(input).Value);
    [Fact]
    public void Parse_ThirteenCharacters_ThrowsInvalidSymbolException() => Assert.Throws<InvalidSymbolException>(() => Symbol.Parse("ABCDEFGHIJKLM"));
    [Theory, MemberData(nameof(InvalidCases))]
    public void Parse_InvalidInput_ThrowsAndTryParseReturnsFalse(string input)
    {
        Assert.Throws<InvalidSymbolException>(() => Symbol.Parse(input));
        Assert.False(Symbol.TryParse(input, out var symbol));
        Assert.Equal(default, symbol);
    }
    [Fact]
    public void Parse_InvalidInput_PreservesOriginalInput()
    {
        var exception = Assert.Throws<InvalidSymbolException>(() => Symbol.Parse(" TS? "));
        Assert.Equal(" TS? ", exception.Input);
    }
    [Fact]
    public void DefaultSymbol_HasNullValue() => Assert.Null(default(Symbol).Value);
}
