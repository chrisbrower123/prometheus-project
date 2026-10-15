using StockData.Domain.Exceptions;
using StockData.Domain.Models;
namespace StockData.Domain.Tests;
public sealed class StockDataExceptionTests
{
    [Fact]
    public void StockDataException_IsAbstract() => Assert.True(typeof(StockDataException).IsAbstract);
    [Fact]
    public void ConcreteExceptions_ArePresentAndDeriveFromBase()
    {
        var types = typeof(StockDataException).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(StockDataException)) && !type.IsAbstract).ToHashSet();
        Assert.Subset(new HashSet<Type> { typeof(InvalidSymbolException), typeof(SymbolNotFoundException), typeof(NoDataAvailableException), typeof(ProviderUnavailableException), typeof(ProviderRateLimitedException) }, types);
    }
    [Fact]
    public void InvalidSymbolException_PreservesInputAndMessage()
    {
        var exception = new InvalidSymbolException("bad");
        Assert.Equal("bad", exception.Input); Assert.Contains("bad", exception.Message);
    }
    [Fact]
    public void SymbolExceptions_PreserveSymbol()
    {
        var symbol = new Symbol("TSLA");
        Assert.Equal(symbol, new SymbolNotFoundException(symbol).Symbol); Assert.Equal(symbol, new NoDataAvailableException(symbol).Symbol);
    }
    [Fact]
    public void ProviderUnavailableException_PreservesMessageAndInnerException()
    {
        var inner = new InvalidOperationException(); var exception = new ProviderUnavailableException("unavailable", inner);
        Assert.Equal("unavailable", exception.Message); Assert.Same(inner, exception.InnerException);
    }
    [Fact]
    public void ProviderRateLimitedException_StoresRetryAfter() => Assert.Equal(TimeSpan.FromSeconds(30), new ProviderRateLimitedException(TimeSpan.FromSeconds(30)).RetryAfter);
    [Fact]
    public void ProviderRateLimitedException_NonPositiveRetryAfter_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => new ProviderRateLimitedException(TimeSpan.Zero));
}
