using System.Text.Json;
using StockData.Domain.Exceptions;
using StockData.Domain.Models;
using StockData.Infrastructure.Providers.Yahoo;
using StockData.Infrastructure.Providers.Yahoo.Dto;
using StockData.Infrastructure.Providers.Yahoo.Mapping;
using StockData.Infrastructure.Tests.TestSupport;

namespace StockData.Infrastructure.Tests;

public sealed class YahooResponseMapperTests
{
    [Fact]
    public void Map_ValidPayload_MapsBarsWithCorrectValuesAndUtcTimestamps()
    {
        var result = YahooResponseMapper.Map(ProviderTestFactory.Symbol, FixtureLoader.LoadResponse("tsla-valid"));
        Assert.Equal(2, result.Bars.Count);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1760000000), result.Bars[0].Timestamp);
        Assert.Equal(40.3m, result.Bars[0].High);
        Assert.Equal(39.1m, result.Bars[0].Low);
        Assert.Equal(0, result.Bars[1].Volume);
    }

    [Fact]
    public void Map_ValidPayload_UsesRequestedSymbol()
    {
        var result = YahooResponseMapper.Map(new Symbol("requested"), FixtureLoader.LoadResponse("tsla-valid"));
        Assert.Equal(new Symbol("requested"), result.Symbol);
    }

    [Theory]
    [InlineData("tsla-valid", "America/New_York")]
    [InlineData("tokyo-valid", "Asia/Tokyo")]
    [InlineData("kolkata-valid", "Asia/Kolkata")]
    public void Map_ExchangeTimezoneName_ResolvesZone(string fixture, string zoneId)
    {
        var result = YahooResponseMapper.Map(ProviderTestFactory.Symbol, FixtureLoader.LoadResponse(fixture));
        Assert.Equal(zoneId, result.ExchangeTimeZone.Id);
    }

    [Fact]
    public void Map_PrecisionFixture_PreservesDecimalLiteralsAndLargeVolume()
    {
        var result = YahooResponseMapper.Map(ProviderTestFactory.Symbol, FixtureLoader.LoadResponse("precision-artifacts"));
        Assert.Equal(40.29999923706055m, result.Bars[0].High);
        Assert.Equal(5000000000L, result.Bars[0].Volume);
    }

    [Fact]
    public void Map_NullFields_SkipsOnlyAffectedBarsAndKeepsAlignment()
    {
        var result = YahooResponseMapper.Map(ProviderTestFactory.Symbol, FixtureLoader.LoadResponse("null-bars"));
        Assert.Single(result.Bars);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1760000000), result.Bars[0].Timestamp);
    }

    [Fact]
    public void Map_ZeroVolume_KeepsBar()
    {
        var result = YahooResponseMapper.Map(ProviderTestFactory.Symbol, FixtureLoader.LoadResponse("tsla-valid"));
        Assert.Equal(2, result.Bars.Count);
        Assert.Equal(0, result.Bars[1].Volume);
    }

    [Fact]
    public void Map_AllBarsNull_ReturnsEmptySeries()
    {
        var response = JsonSerializer.Deserialize<YahooChartResponse>(Payloads.Result("[{\"meta\":{\"exchangeTimezoneName\":\"UTC\",\"gmtoffset\":0},\"timestamp\":[1],\"indicators\":{\"quote\":[{\"high\":[null],\"low\":[null],\"volume\":[null]}]}}]"), YahooJson.Options)!;
        Assert.Empty(YahooResponseMapper.Map(ProviderTestFactory.Symbol, response).Bars);
    }

    [Theory]
    [InlineData(40, 41, 1)]
    [InlineData(-1, 1, 1)]
    [InlineData(2, 1, -1)]
    public void Map_AnomalousBars_AreSkipped(decimal high, decimal low, long volume)
    {
        var response = JsonSerializer.Deserialize<YahooChartResponse>(Payloads.Result($"[{{\"meta\":{{\"exchangeTimezoneName\":\"UTC\",\"gmtoffset\":0}},\"timestamp\":[1,2],\"indicators\":{{\"quote\":[{{\"high\":[{high},2],\"low\":[{low},1],\"volume\":[{volume},1]}}]}}}}]"), YahooJson.Options)!;
        Assert.Single(YahooResponseMapper.Map(ProviderTestFactory.Symbol, response).Bars);
    }

    [Theory]
    [InlineData("Not Found")]
    [InlineData("not found")]
    [InlineData("NOT FOUND")]
    public void Map_NotFoundError_ThrowsSymbolNotFoundException(string code)
    {
        var exception = Assert.Throws<SymbolNotFoundException>(() => YahooResponseMapper.Map(ProviderTestFactory.Symbol, JsonSerializer.Deserialize<YahooChartResponse>(Payloads.Error(code), YahooJson.Options)!));
        Assert.Equal(ProviderTestFactory.Symbol, exception.Symbol);
    }

    [Fact]
    public void Map_OtherErrorCode_ThrowsProviderUnavailableWithoutRawPayload()
    {
        var exception = Assert.Throws<ProviderUnavailableException>(() => YahooResponseMapper.Map(ProviderTestFactory.Symbol, JsonSerializer.Deserialize<YahooChartResponse>(Payloads.Error("Bad Request", "secret-body"), YahooJson.Options)!));
        Assert.Contains("Bad Request", exception.Message);
        Assert.DoesNotContain("secret-body", exception.Message);
    }

    [Theory]
    [InlineData("{\"chart\":{\"result\":null,\"error\":null}}")]
    [InlineData("{\"chart\":{\"result\":[],\"error\":null}}")]
    [InlineData("{\"chart\":{\"result\":[{\"timestamp\":[1],\"indicators\":{\"quote\":[{}]}}],\"error\":null}}")]
    [InlineData("{\"chart\":{\"result\":[{\"meta\":{},\"timestamp\":[1],\"indicators\":{}}],\"error\":null}}")]
    public void Map_MalformedStructure_ThrowsProviderUnavailable(string payload)
    {
        Assert.Throws<ProviderUnavailableException>(() => YahooResponseMapper.Map(ProviderTestFactory.Symbol, JsonSerializer.Deserialize<YahooChartResponse>(payload, YahooJson.Options)!));
    }

    [Fact]
    public void Map_MultipleResults_UsesFirst()
    {
        var response = JsonSerializer.Deserialize<YahooChartResponse>(Payloads.Result("[{\"meta\":{\"exchangeTimezoneName\":\"UTC\",\"gmtoffset\":0},\"timestamp\":[1],\"indicators\":{\"quote\":[{\"high\":[2],\"low\":[1],\"volume\":[1]}]}},{\"meta\":{\"exchangeTimezoneName\":\"Asia/Tokyo\",\"gmtoffset\":32400},\"timestamp\":[2],\"indicators\":{\"quote\":[{\"high\":[3],\"low\":[2],\"volume\":[2]}]}}]"), YahooJson.Options)!;
        Assert.Equal(TimeZoneInfo.Utc.Id, YahooResponseMapper.Map(ProviderTestFactory.Symbol, response).ExchangeTimeZone.Id);
    }

    [Fact]
    public void Map_MissingTimestampArray_ReturnsEmptySeries()
    {
        var response = JsonSerializer.Deserialize<YahooChartResponse>(Payloads.Result("[{\"meta\":{\"exchangeTimezoneName\":\"UTC\",\"gmtoffset\":0},\"indicators\":{\"quote\":[{\"high\":[2],\"low\":[1],\"volume\":[1]}]}}]"), YahooJson.Options)!;
        Assert.Empty(YahooResponseMapper.Map(ProviderTestFactory.Symbol, response).Bars);
    }

    [Fact]
    public void Map_MismatchedArrayLengths_TruncatesToShortest()
    {
        var response = JsonSerializer.Deserialize<YahooChartResponse>(Payloads.Result("[{\"meta\":{\"exchangeTimezoneName\":\"UTC\",\"gmtoffset\":0},\"timestamp\":[1,2,3],\"indicators\":{\"quote\":[{\"high\":[2,3],\"low\":[1,2],\"volume\":[1,2]}]}}]"), YahooJson.Options)!;
        Assert.Equal(2, YahooResponseMapper.Map(ProviderTestFactory.Symbol, response).Bars.Count);
    }

    [Theory]
    [InlineData("Unknown/Zone")]
    [InlineData(null)]
    public void Map_UnknownOrMissingTimezoneName_FallsBackToGmtOffset(string? zoneId)
    {
        var name = zoneId is null ? "null" : $"\"{zoneId}\"";
        var response = JsonSerializer.Deserialize<YahooChartResponse>(Payloads.Result($"[{{\"meta\":{{\"exchangeTimezoneName\":{name},\"gmtoffset\":3600}},\"timestamp\":[1],\"indicators\":{{\"quote\":[{{\"high\":[2],\"low\":[1],\"volume\":[1]}}]}}}}]"), YahooJson.Options)!;
        Assert.Equal(TimeSpan.FromHours(1), YahooResponseMapper.Map(ProviderTestFactory.Symbol, response).ExchangeTimeZone.BaseUtcOffset);
    }

    [Fact]
    public void Map_UnknownExtraProperties_AreIgnored()
    {
        var result = YahooResponseMapper.Map(ProviderTestFactory.Symbol, FixtureLoader.LoadResponse("extra-fields"));
        Assert.Equal(2, result.Bars.Count);
    }
}
