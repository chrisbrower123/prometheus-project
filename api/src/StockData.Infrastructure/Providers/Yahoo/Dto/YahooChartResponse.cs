using System.Text.Json.Serialization;
namespace StockData.Infrastructure.Providers.Yahoo.Dto;
internal sealed record YahooChartResponse([property: JsonPropertyName("chart")] YahooChart Chart);
internal sealed record YahooChart([property: JsonPropertyName("result")] YahooChartResult?[]? Result, [property: JsonPropertyName("error")] YahooChartError? Error);
internal sealed record YahooChartResult([property: JsonPropertyName("meta")] YahooMeta Meta, [property: JsonPropertyName("timestamp")] long[]? Timestamp, [property: JsonPropertyName("indicators")] YahooIndicators Indicators);
internal sealed record YahooMeta([property: JsonPropertyName("symbol")] string? Symbol, [property: JsonPropertyName("exchangeTimezoneName")] string? ExchangeTimezoneName, [property: JsonPropertyName("gmtoffset")] int GmtOffset);
internal sealed record YahooIndicators([property: JsonPropertyName("quote")] YahooQuote[] Quote);
internal sealed record YahooQuote([property: JsonPropertyName("high")] decimal?[]? High, [property: JsonPropertyName("low")] decimal?[]? Low, [property: JsonPropertyName("volume")] long?[]? Volume);
internal sealed record YahooChartError([property: JsonPropertyName("code")] string? Code, [property: JsonPropertyName("description")] string? Description);
