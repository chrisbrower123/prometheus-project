using System.Net;
using System.Net.Http.Headers;
using StockData.Domain.Exceptions;
using StockData.Domain.Models;
using StockData.Infrastructure.Providers.Yahoo;
using StockData.Infrastructure.Tests.TestSupport;

namespace StockData.Infrastructure.Tests;

public sealed class YahooMarketDataProviderTests
{
    private static StubHttpMessageHandler JsonHandler(string body, HttpStatusCode status = HttpStatusCode.OK) =>
        new((_, _) => Task.FromResult(ResponseFactory.Json(status, body)));

    [Fact]
    public async Task GetIntradaySeries_ValidRequest_SendsSingleGetToChartPathWithInterval()
    {
        var handler = JsonHandler(Payloads.Valid);
        await ProviderTestFactory.Create(handler).GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, IntradayInterval.FifteenMinutes, CancellationToken.None);
        Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, handler.Requests[0].Method);
        Assert.Contains("/v8/finance/chart/TSLA", handler.Requests[0].RequestUri!.AbsolutePath);
        Assert.Contains("interval=15m", handler.Requests[0].RequestUri!.Query);
    }

    [Fact]
    public async Task GetIntradaySeries_Period_UsesUnixSecondsFromInstantsRegardlessOfOffset()
    {
        var handler = JsonHandler(Payloads.Valid);
        var from = new DateTimeOffset(2026, 1, 1, 1, 0, 0, TimeSpan.FromHours(1));
        var to = from.AddHours(1);
        await ProviderTestFactory.Create(handler).GetIntradaySeriesAsync(ProviderTestFactory.Symbol, from, to, IntradayInterval.FifteenMinutes, CancellationToken.None);
        Assert.Contains($"period1={from.ToUnixTimeSeconds()}", handler.Requests[0].RequestUri!.Query);
        Assert.Contains($"period2={to.ToUnixTimeSeconds()}", handler.Requests[0].RequestUri!.Query);
    }

    [Theory]
    [InlineData("^GSPC", "%5EGSPC")]
    [InlineData("EURUSD=X", "EURUSD=X")]
    [InlineData("BRK.B", "BRK.B")]
    [InlineData("BTC-USD", "BTC-USD")]
    public async Task GetIntradaySeries_Symbol_IsUrlEncodedInPath(string symbol, string expected)
    {
        var handler = JsonHandler(Payloads.Valid);
        await ProviderTestFactory.Create(handler).GetIntradaySeriesAsync(new Symbol(symbol), ProviderTestFactory.From, ProviderTestFactory.To, IntradayInterval.FifteenMinutes, CancellationToken.None);
        Assert.Contains(expected, handler.Requests[0].RequestUri!.AbsoluteUri);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task GetIntradaySeries_FromNotBeforeTo_ThrowsArgumentExceptionAndSendsNothing(int offsetHours)
    {
        var handler = JsonHandler(Payloads.Valid);
        var from = ProviderTestFactory.To.AddHours(offsetHours);
        await Assert.ThrowsAsync<ArgumentException>(() => ProviderTestFactory.Create(handler).GetIntradaySeriesAsync(ProviderTestFactory.Symbol, from, ProviderTestFactory.To, IntradayInterval.FifteenMinutes, CancellationToken.None));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task GetIntradaySeries_UndefinedInterval_ThrowsArgumentOutOfRange()
    {
        var handler = JsonHandler(Payloads.Valid);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ProviderTestFactory.Create(handler).GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, (IntradayInterval)999, CancellationToken.None));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task GetIntradaySeries_Success_ReturnsMappedSeries()
    {
        var result = await ProviderTestFactory.Create(JsonHandler(Payloads.Valid)).GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, IntradayInterval.FifteenMinutes, CancellationToken.None);
        Assert.Equal(ProviderTestFactory.Symbol, result.Symbol);
    }

    [Theory]
    [InlineData("Not Found")]
    [InlineData("")]
    [InlineData("<html>not found</html>")]
    public async Task GetIntradaySeries_404_ThrowsSymbolNotFound(string body)
    {
        await Assert.ThrowsAsync<SymbolNotFoundException>(() => ProviderTestFactory.Create(JsonHandler(body, HttpStatusCode.NotFound)).GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, IntradayInterval.FifteenMinutes, CancellationToken.None));
    }

    [Fact]
    public async Task GetIntradaySeries_200WithNotFoundPayload_ThrowsSymbolNotFound()
    {
        await Assert.ThrowsAsync<SymbolNotFoundException>(() => ProviderTestFactory.Create(JsonHandler(Payloads.Error("Not Found"))).GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, IntradayInterval.FifteenMinutes, CancellationToken.None));
    }

    [Fact]
    public async Task GetIntradaySeries_429_ThrowsProviderRateLimited()
    {
        await Assert.ThrowsAsync<ProviderRateLimitedException>(() => ProviderTestFactory.Create(JsonHandler("", HttpStatusCode.TooManyRequests)).GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, IntradayInterval.FifteenMinutes, CancellationToken.None));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    [InlineData(HttpStatusCode.Redirect)]
    public async Task GetIntradaySeries_FailureStatus_ThrowsProviderUnavailable(HttpStatusCode status)
    {
        await Assert.ThrowsAsync<ProviderUnavailableException>(() => ProviderTestFactory.Create(JsonHandler("secret-body", status)).GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, IntradayInterval.FifteenMinutes, CancellationToken.None));
    }

    [Theory]
    [InlineData("30", 30)]
    [InlineData("garbage", -1)]
    [InlineData("0", -1)]
    [InlineData("86400", 3600)]
    public async Task GetIntradaySeries_429RetryAfter_ParsesAndCaps(string header, int seconds)
    {
        var handler = new StubHttpMessageHandler((_, _) => { var response = ResponseFactory.Json(HttpStatusCode.TooManyRequests, ""); response.Headers.TryAddWithoutValidation("Retry-After", header); return Task.FromResult(response); });
        var exception = await Assert.ThrowsAsync<ProviderRateLimitedException>(() => ProviderTestFactory.Create(handler).GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, IntradayInterval.FifteenMinutes, CancellationToken.None));
        Assert.Equal(seconds < 0 ? null : TimeSpan.FromSeconds(seconds), exception.RetryAfter);
    }

    [Fact]
    public async Task GetIntradaySeries_HttpRequestException_ThrowsProviderUnavailableWithInner()
    {
        var inner = new HttpRequestException("transport");
        var handler = new StubHttpMessageHandler((_, _) => throw inner);
        var exception = await Assert.ThrowsAsync<ProviderUnavailableException>(() => ProviderTestFactory.Create(handler).GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, IntradayInterval.FifteenMinutes, CancellationToken.None));
        Assert.Same(inner, exception.InnerException);
    }

    [Fact]
    public async Task GetIntradaySeries_TimeoutWithoutCallerCancellation_ThrowsProviderUnavailable()
    {
        var handler = new StubHttpMessageHandler((_, _) => throw new TaskCanceledException("timeout"));
        await Assert.ThrowsAsync<ProviderUnavailableException>(() => ProviderTestFactory.Create(handler).GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, IntradayInterval.FifteenMinutes, CancellationToken.None));
    }

    [Fact]
    public async Task GetIntradaySeries_CallerCancellation_ThrowsOperationCanceledNotWrapped()
    {
        using var source = new CancellationTokenSource();
        await source.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ProviderTestFactory.Create(JsonHandler(Payloads.Valid)).GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, IntradayInterval.FifteenMinutes, source.Token));
    }

    [Fact]
    public async Task GetIntradaySeries_CancellationToken_IsForwardedToHandler()
    {
        using var source = new CancellationTokenSource();
        var handler = JsonHandler(Payloads.Valid);
        await ProviderTestFactory.Create(handler).GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, IntradayInterval.FifteenMinutes, source.Token);
        Assert.Equal(source.Token, handler.CancellationTokens[0]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("<html>consent</html>")]
    [InlineData("null")]
    public async Task GetIntradaySeries_UnusableBody_ThrowsProviderUnavailable(string body)
    {
        await Assert.ThrowsAsync<ProviderUnavailableException>(() => ProviderTestFactory.Create(JsonHandler(body)).GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, IntradayInterval.FifteenMinutes, CancellationToken.None));
    }

    [Fact]
    public async Task GetIntradaySeries_WrongContentTypeWithValidJson_IsTolerated()
    {
        var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(ResponseFactory.Json(HttpStatusCode.OK, Payloads.Valid, "text/plain")));
        await ProviderTestFactory.Create(handler).GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, IntradayInterval.FifteenMinutes, CancellationToken.None);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, "distinct-body")]
    [InlineData(HttpStatusCode.OK, "malformed-json")]
    public async Task GetIntradaySeries_Failures_DoNotLeakUrlOrBodyInMessage(HttpStatusCode status, string body)
    {
        var exception = await Assert.ThrowsAsync<ProviderUnavailableException>(() => ProviderTestFactory.Create(JsonHandler(body, status)).GetIntradaySeriesAsync(ProviderTestFactory.Symbol, ProviderTestFactory.From, ProviderTestFactory.To, IntradayInterval.FifteenMinutes, CancellationToken.None));
        Assert.DoesNotContain(body, exception.Message);
        Assert.DoesNotContain("query1.finance.yahoo.com", exception.Message);
    }
}
