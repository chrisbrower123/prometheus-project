using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using StockData.Domain.Exceptions;
using StockData.Domain.Abstractions;
using StockData.Domain.Models;
using StockData.Infrastructure.Providers.Yahoo.Dto;
using StockData.Infrastructure.Providers.Yahoo.Mapping;
namespace StockData.Infrastructure.Providers.Yahoo;
internal sealed class YahooMarketDataProvider(HttpClient httpClient, IOptions<YahooFinanceOptions> options, TimeProvider timeProvider) : IMarketDataProvider
{
    public async Task<IntradaySeries> GetIntradaySeriesAsync(Symbol symbol, DateTimeOffset from, DateTimeOffset until, IntradayInterval interval, CancellationToken cancellationToken)
    {
        _ = options;
        httpClient.Timeout = Timeout.InfiniteTimeSpan;
        if (from >= until) throw new ArgumentException("The start instant must be before the end instant.");
        var intervalValue = interval switch
        {
            IntradayInterval.FifteenMinutes => "15m",
            _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, "The interval is not supported."),
        };

        cancellationToken.ThrowIfCancellationRequested();
        var encodedSymbol = Uri.EscapeDataString(symbol.Value ?? string.Empty).Replace("%3D", "=", StringComparison.OrdinalIgnoreCase);
        var requestUri = $"v8/finance/chart/{encodedSymbol}?period1={from.ToUnixTimeSeconds()}&period2={until.ToUnixTimeSeconds()}&interval={intervalValue}";

        HttpResponseMessage response;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new ProviderUnavailableException("Yahoo provider request failed.", exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ProviderUnavailableException("Yahoo provider request timed out.", exception);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException exception)
        {
            throw new ProviderUnavailableException("Yahoo provider request was cancelled.", exception);
        }

        using (response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new SymbolNotFoundException(symbol);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                throw new ProviderRateLimitedException(ParseRetryAfter(response));
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new ProviderUnavailableException("Yahoo provider returned an unsuccessful response.");
            }

            string body;
            try
            {
                body = await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TaskCanceledException exception)
            {
                throw new ProviderUnavailableException("Yahoo provider response timed out.", exception);
            }

            YahooChartResponse? payload;
            try
            {
                payload = JsonSerializer.Deserialize<YahooChartResponse>(body, YahooJson.Options);
            }
            catch (JsonException exception)
            {
                throw new ProviderUnavailableException("Yahoo provider returned malformed JSON.", exception);
            }

            try
            {
                return YahooResponseMapper.Map(symbol, payload);
            }
            catch (ArgumentNullException exception)
            {
                throw new ProviderUnavailableException("Yahoo provider returned an unusable response.", exception);
            }
        }
    }

    private TimeSpan? ParseRetryAfter(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Retry-After", out var values)) return null;
        var value = values.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(value)) return null;

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
        {
            return seconds > 0 ? TimeSpan.FromSeconds(Math.Min(seconds, 3600)) : null;
        }

        if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var retryAt)) return null;
        var delay = retryAt - timeProvider.GetUtcNow();
        if (delay <= TimeSpan.Zero) return null;
        return delay > TimeSpan.FromHours(1) ? TimeSpan.FromHours(1) : delay;
    }
}
