using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using StockData.Domain.Abstractions;
using StockData.Domain.Exceptions;
using StockData.Domain.Models;
using StockData.Domain.Services;

namespace StockData.Domain.Tests;

public sealed class DailySummaryServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 2, 15, 14, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetDailySummariesAsync_UsesCurrentInstantAndOneMonthWindow()
    {
        var provider = Substitute.For<IMarketDataProvider>();
        var aggregator = Substitute.For<IDailyAggregator>();
        var timeProvider = new FakeTimeProvider(Now);
        var series = new IntradaySeries(new Symbol("TSLA"), TimeZoneInfo.Utc, new[]
        {
            new IntradayBar(Now, 110m, 100m, 10),
        });
        provider.GetIntradaySeriesAsync(Arg.Any<Symbol>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<IntradayInterval>(), Arg.Any<CancellationToken>())
            .Returns(series);
        aggregator.Aggregate(series).Returns(Array.Empty<DailySummary>());

        await new DailySummaryService(provider, aggregator, timeProvider)
            .GetDailySummariesAsync(new Symbol("TSLA"), CancellationToken.None);

        await provider.Received(1).GetIntradaySeriesAsync(
            new Symbol("TSLA"),
            Now.AddMonths(-1),
            Now,
            IntradayInterval.FifteenMinutes,
            CancellationToken.None);
    }

    [Fact]
    public async Task GetDailySummariesAsync_DefaultSymbol_ThrowsInvalidSymbolException()
    {
        var provider = Substitute.For<IMarketDataProvider>();
        var aggregator = Substitute.For<IDailyAggregator>();
        var service = new DailySummaryService(provider, aggregator, new FakeTimeProvider(Now));

        await Assert.ThrowsAsync<InvalidSymbolException>(() =>
            service.GetDailySummariesAsync(default, CancellationToken.None));

        await provider.DidNotReceiveWithAnyArgs().GetIntradaySeriesAsync(default, default, default, default, default);
        aggregator.DidNotReceiveWithAnyArgs().Aggregate(default!);
    }

    [Fact]
    public async Task GetDailySummariesAsync_EmptySeries_ThrowsNoDataAvailableException()
    {
        var provider = Substitute.For<IMarketDataProvider>();
        var aggregator = Substitute.For<IDailyAggregator>();
        var symbol = new Symbol("TSLA");
        provider.GetIntradaySeriesAsync(Arg.Any<Symbol>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<IntradayInterval>(), Arg.Any<CancellationToken>())
            .Returns(new IntradaySeries(symbol, TimeZoneInfo.Utc, Array.Empty<IntradayBar>()));
        var service = new DailySummaryService(provider, aggregator, new FakeTimeProvider(Now));

        await Assert.ThrowsAsync<NoDataAvailableException>(() =>
            service.GetDailySummariesAsync(symbol, CancellationToken.None));

        aggregator.DidNotReceiveWithAnyArgs().Aggregate(default!);
    }

    [Fact]
    public async Task GetDailySummariesAsync_ProviderFailure_PropagatesSameException()
    {
        var provider = Substitute.For<IMarketDataProvider>();
        var aggregator = Substitute.For<IDailyAggregator>();
        var failure = new ProviderUnavailableException("unavailable");
        provider.GetIntradaySeriesAsync(Arg.Any<Symbol>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<IntradayInterval>(), Arg.Any<CancellationToken>())
            .Returns<Task<IntradaySeries>>(_ => throw failure);
        var service = new DailySummaryService(provider, aggregator, new FakeTimeProvider(Now));

        var actual = await Assert.ThrowsAsync<ProviderUnavailableException>(() =>
            service.GetDailySummariesAsync(new Symbol("TSLA"), CancellationToken.None));

        Assert.Same(failure, actual);
    }

    [Fact]
    public async Task GetDailySummariesAsync_DataAvailable_ReturnsAggregatorOutput()
    {
        var provider = Substitute.For<IMarketDataProvider>();
        var aggregator = Substitute.For<IDailyAggregator>();
        var series = new IntradaySeries(new Symbol("TSLA"), TimeZoneInfo.Utc, new[]
        {
            new IntradayBar(Now, 110m, 100m, 10),
        });
        var expected = new[] { new DailySummary(new DateOnly(2026, 2, 15), 100m, 110m, 10) };
        provider.GetIntradaySeriesAsync(Arg.Any<Symbol>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<IntradayInterval>(), Arg.Any<CancellationToken>())
            .Returns(series);
        aggregator.Aggregate(series).Returns(expected);

        var actual = await new DailySummaryService(provider, aggregator, new FakeTimeProvider(Now))
            .GetDailySummariesAsync(new Symbol("TSLA"), CancellationToken.None);

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task GetDailySummariesAsync_PreCancelledToken_ThrowsOperationCanceledException()
    {
        var provider = Substitute.For<IMarketDataProvider>();
        var aggregator = Substitute.For<IDailyAggregator>();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var service = new DailySummaryService(provider, aggregator, new FakeTimeProvider(Now));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.GetDailySummariesAsync(new Symbol("TSLA"), cancellation.Token));

        await provider.DidNotReceiveWithAnyArgs().GetIntradaySeriesAsync(default, default, default, default, default);
    }
}
