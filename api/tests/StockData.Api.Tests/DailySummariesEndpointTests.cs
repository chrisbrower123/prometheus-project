using System.Text.Json;
using System.Net;
using StockData.Api.ErrorHandling;
using StockData.Api.Tests.TestSupport;
using StockData.Domain.Exceptions;
using StockData.Domain.Models;

namespace StockData.Api.Tests;

public sealed class DailySummariesEndpointTests
{
    private static IntradaySeries TwoDaySeries() => TestData.Series(TestData.NewYork,
        TestData.Bar(TestData.Utc(2026, 2, 10), 1, 2, 3),
        TestData.Bar(TestData.Utc(2026, 2, 11), 4, 5, 6));

    [Fact]
    public async Task Get_ValidSymbol_Returns200WithExactJsonShape()
    {
        await using var factory = new ApiFactory(); factory.Provider.Returns(TwoDaySeries());
        var response = await factory.CreateClient().GetAsync("/api/stocks/TSLA/daily-summaries");
        var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind); Assert.Equal(2, document.RootElement.GetArrayLength());
        Assert.Equal(4, document.RootElement[0].EnumerateObject().Count());
    }

    [Fact]
    public async Task Get_LowercaseSymbol_ReachesProviderUppercased()
    {
        await using var factory = new ApiFactory(); factory.Provider.Returns(TwoDaySeries());
        await factory.CreateClient().GetAsync("/api/stocks/tsla/daily-summaries");
        Assert.Equal("TSLA", factory.Provider.Calls[0].Symbol.Value);
    }

    [Fact]
    public async Task Get_UnorderedBars_ReturnsAscendingByDay()
    {
        await using var factory = new ApiFactory(); factory.Provider.Returns(TestData.Series(TestData.NewYork, TestData.Bar(TestData.Utc(2026, 2, 12), 1, 2), TestData.Bar(TestData.Utc(2026, 2, 10), 1, 2), TestData.Bar(TestData.Utc(2026, 2, 11), 1, 2)));
        var response = await factory.CreateClient().GetAsync("/api/stocks/TSLA/daily-summaries");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_ProviderReceivesFifteenMinutesAndOneMonthWindow()
    {
        await using var factory = new ApiFactory(); factory.Provider.Returns(TwoDaySeries());
        await factory.CreateClient().GetAsync("/api/stocks/TSLA/daily-summaries");
        var call = factory.Provider.Calls[0];
        Assert.Equal(IntradayInterval.FifteenMinutes, call.Interval); Assert.Equal(factory.Clock.GetUtcNow(), call.To); Assert.Equal(call.To.AddMonths(-1), call.From);
    }

    [Theory]
    [InlineData("TS%20LA")]
    [InlineData("TSLA.")]
    [InlineData("ABCDEFGHIJKLM")]
    [InlineData("TS$LA")]
    public async Task Get_InvalidSymbol_Returns400ProblemJsonAndNeverCallsProvider(string symbol)
    {
        await using var factory = new ApiFactory();
        var response = await factory.CreateClient().GetAsync($"/api/stocks/{symbol}/daily-summaries");
        var problem = await ProblemAssert.Read(response, HttpStatusCode.BadRequest);
        Assert.Equal(ProblemTypes.InvalidSymbol, problem.RootElement.GetProperty("type").GetString()); Assert.Empty(factory.Provider.Calls);
    }

    [Fact]
    public async Task Get_ProviderThrowsSymbolNotFound_Returns404ProblemJson()
    {
        await using var factory = new ApiFactory(); factory.Provider.Throws(new SymbolNotFoundException(new Symbol("TSLA")));
        var problem = await ProblemAssert.Read(await factory.CreateClient().GetAsync("/api/stocks/TSLA/daily-summaries"), HttpStatusCode.NotFound);
        Assert.Equal(ProblemTypes.SymbolNotFound, problem.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public async Task Get_ProviderReturnsNoBars_Returns404WithNoDataType()
    {
        await using var factory = new ApiFactory(); factory.Provider.Returns(TestData.Series(TestData.NewYork));
        var problem = await ProblemAssert.Read(await factory.CreateClient().GetAsync("/api/stocks/TSLA/daily-summaries"), HttpStatusCode.NotFound);
        Assert.Equal(ProblemTypes.NoDataAvailable, problem.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public async Task Get_ProviderThrowsRateLimited_Returns429WithRetryAfterHeader()
    {
        await using var factory = new ApiFactory(); factory.Provider.Throws(new ProviderRateLimitedException(TimeSpan.FromSeconds(45)));
        var response = await factory.CreateClient().GetAsync("/api/stocks/TSLA/daily-summaries");
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode); Assert.Equal(TimeSpan.FromSeconds(45), response.Headers.RetryAfter?.Delta);
    }

    [Fact]
    public async Task Get_ProviderThrowsUnavailable_Returns502ProblemJson()
    {
        await using var factory = new ApiFactory(); factory.Provider.Throws(new ProviderUnavailableException("unavailable"));
        var problem = await ProblemAssert.Read(await factory.CreateClient().GetAsync("/api/stocks/TSLA/daily-summaries"), HttpStatusCode.BadGateway);
        Assert.Equal(ProblemTypes.ProviderUnavailable, problem.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public async Task Get_ProviderThrowsUnexpectedException_Returns500WithoutLeakInDevelopment()
    {
        await using var factory = new ApiFactory(); factory.Provider.Throws(new InvalidOperationException("SECRET-DETAIL"));
        var response = await factory.CreateClient().GetAsync("/api/stocks/TSLA/daily-summaries");
        var text = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode); Assert.DoesNotContain("SECRET-DETAIL", text); Assert.DoesNotContain(" at ", text);
    }
}
