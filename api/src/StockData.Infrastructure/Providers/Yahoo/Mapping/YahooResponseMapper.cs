using StockData.Domain.Exceptions;
using StockData.Domain.Models;
using StockData.Infrastructure.Providers.Yahoo.Dto;
namespace StockData.Infrastructure.Providers.Yahoo.Mapping;
internal static class YahooResponseMapper
{
    internal static IntradaySeries Map(Symbol symbol, YahooChartResponse? response)
    {
        ArgumentNullException.ThrowIfNull(response);
        var chart = response.Chart ?? throw new ProviderUnavailableException("Yahoo response was malformed.");

        if (chart.Error is not null)
        {
            var code = chart.Error.Code ?? string.Empty;
            if (string.Equals(code, "Not Found", StringComparison.OrdinalIgnoreCase))
            {
                throw new SymbolNotFoundException(symbol);
            }

            throw new ProviderUnavailableException($"Yahoo provider error: {code}.");
        }

        var result = chart.Result is { Length: > 0 } ? chart.Result[0] : null;
        if (result is null) throw new ProviderUnavailableException("Yahoo response was malformed.");
        var meta = result.Meta ?? throw new ProviderUnavailableException("Yahoo response was malformed.");
        var quote = result.Indicators?.Quote is { Length: > 0 } quotes
            ? quotes[0]
            : throw new ProviderUnavailableException("Yahoo response was malformed.");
        var timeZone = ResolveTimeZone(meta);

        if (result.Timestamp is null)
        {
            return new IntradaySeries(symbol, timeZone, Array.Empty<IntradayBar>());
        }

        var high = quote.High ?? Array.Empty<decimal?>();
        var low = quote.Low ?? Array.Empty<decimal?>();
        var volume = quote.Volume ?? Array.Empty<long?>();
        var count = Math.Min(result.Timestamp.Length, Math.Min(high.Length, Math.Min(low.Length, volume.Length)));
        var bars = new List<IntradayBar>(count);

        for (var index = 0; index < count; index++)
        {
            var barHigh = high[index];
            var barLow = low[index];
            var barVolume = volume[index];
            if (barHigh is null || barLow is null || barVolume is null || barHigh < barLow || barLow < 0 || barVolume < 0)
            {
                continue;
            }

            bars.Add(new IntradayBar(DateTimeOffset.FromUnixTimeSeconds(result.Timestamp[index]), barHigh.Value, barLow.Value, barVolume.Value));
        }

        return new IntradaySeries(symbol, timeZone, bars);
    }

    private static TimeZoneInfo ResolveTimeZone(YahooMeta meta)
    {
        if (!string.IsNullOrWhiteSpace(meta.ExchangeTimezoneName))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(meta.ExchangeTimezoneName);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        if (meta.GmtOffset is not int offsetSeconds)
        {
            throw new ProviderUnavailableException("Yahoo response timezone was unusable.");
        }

        try
        {
            return TimeZoneInfo.CreateCustomTimeZone("YahooFixedOffset", TimeSpan.FromSeconds(offsetSeconds), "Yahoo fixed offset", "Yahoo fixed offset");
        }
        catch (ArgumentException)
        {
            throw new ProviderUnavailableException("Yahoo response timezone was unusable.");
        }
    }
}
