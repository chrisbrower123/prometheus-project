using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StockData.Api.ErrorHandling;
using StockData.Api.Tests.TestSupport;
using StockData.Domain.Exceptions;
using StockData.Domain.Models;

namespace StockData.Api.Tests;

public sealed class StockDataExceptionHandlerTests
{
    public static TheoryData<Exception, int, string> DomainCases => new()
    {
        { new InvalidSymbolException("TS LA"), 400, ProblemTypes.InvalidSymbol },
        { new SymbolNotFoundException(new Symbol("TSLA")), 404, ProblemTypes.SymbolNotFound },
        { new NoDataAvailableException(new Symbol("TSLA")), 404, ProblemTypes.NoDataAvailable },
        { new ProviderRateLimitedException(TimeSpan.FromSeconds(30)), 429, ProblemTypes.RateLimited },
        { new ProviderUnavailableException("unavailable"), 502, ProblemTypes.ProviderUnavailable },
    };

    [Theory]
    [MemberData(nameof(DomainCases))]
    public async Task TryHandleAsync_DomainException_ReturnsTrueWithMappedStatus(Exception exception, int status, string _)
    {
        var (context, _, _, handler) = HandlerHarness.Create();
        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);
        Assert.True(handled);
        Assert.Equal(status, context.Response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(DomainCases))]
    public async Task TryHandleAsync_DomainException_WritesProblemJsonWithRequiredFields(Exception exception, int _, string type)
    {
        var (context, body, _, handler) = HandlerHarness.Create();
        await handler.TryHandleAsync(context, exception, CancellationToken.None);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        Assert.Contains(type, body.ToArray().Length == 0 ? string.Empty : "traceId");
    }

    [Theory]
    [InlineData(30, "30")]
    [InlineData(1.2, "2")]
    [InlineData(0.000001, "1")]
    public async Task TryHandleAsync_RateLimited_SetsRetryAfterInWholeSecondsRoundedUp(double seconds, string expected)
    {
        var (context, _, _, handler) = HandlerHarness.Create();
        await handler.TryHandleAsync(context, new ProviderRateLimitedException(TimeSpan.FromSeconds(seconds)), CancellationToken.None);
        Assert.Equal(expected, context.Response.Headers.RetryAfter.ToString());
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundAndNoData_HaveDistinctTypes()
    {
        var first = HandlerHarness.Create(); var second = HandlerHarness.Create();
        await first.Handler.TryHandleAsync(first.Context, new SymbolNotFoundException(new Symbol("TSLA")), CancellationToken.None);
        await second.Handler.TryHandleAsync(second.Context, new NoDataAvailableException(new Symbol("TSLA")), CancellationToken.None);
        Assert.NotEqual(first.Context.Response.Headers.ContentType, second.Context.Response.Headers.ContentType);
    }

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_Writes500WithoutLeakingDetails()
    {
        var (context, body, _, handler) = HandlerHarness.Create();
        await handler.TryHandleAsync(context, new InvalidOperationException("SECRET-DETAIL"), CancellationToken.None);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.DoesNotContain("SECRET-DETAIL", System.Text.Encoding.UTF8.GetString(body.ToArray()));
    }

    [Fact]
    public async Task TryHandleAsync_ProviderUnavailable_DoesNotLeakUpstreamInformation()
    {
        var (context, body, _, handler) = HandlerHarness.Create();
        await handler.TryHandleAsync(context, new ProviderUnavailableException("GET https://query1.finance.yahoo.com/v8/finance/chart/TSLA failed: <html>UPSTREAM-BODY</html>", new InvalidOperationException("INNER-SECRET")), CancellationToken.None);
        var text = System.Text.Encoding.UTF8.GetString(body.ToArray());
        Assert.DoesNotContain("query1.finance.yahoo.com", text);
        Assert.DoesNotContain("UPSTREAM-BODY", text);
        Assert.DoesNotContain("INNER-SECRET", text);
    }

    [Fact]
    public async Task TryHandleAsync_ClientAbort_ReturnsFalseWritesNothingAndLogsNoError()
    {
        var (context, body, logger, handler) = HandlerHarness.Create();
        using var source = new CancellationTokenSource();
        await source.CancelAsync(); context.RequestAborted = source.Token;
        var handled = await handler.TryHandleAsync(context, new OperationCanceledException(), CancellationToken.None);
        Assert.False(handled); Assert.Empty(body.ToArray()); Assert.DoesNotContain(logger.Levels, level => level >= LogLevel.Error);
    }

    [Theory]
    [InlineData(true, LogLevel.Warning)]
    [InlineData(false, LogLevel.Error)]
    public async Task TryHandleAsync_LogLevels_WarningForDomainErrorForUnexpected(bool domain, LogLevel expected)
    {
        var (context, _, logger, handler) = HandlerHarness.Create();
        await handler.TryHandleAsync(context, domain ? new SymbolNotFoundException(new Symbol("TSLA")) : new InvalidOperationException(), CancellationToken.None);
        Assert.Single(logger.Levels); Assert.Equal(expected, logger.Levels[0]);
    }
}
