using System.Text.Json.Serialization;
namespace StockData.Api.Contracts;
/// <summary>Daily summary API response.</summary>
public sealed record DailySummaryResponse([property: JsonPropertyName("day")] string Day, [property: JsonPropertyName("lowAverage")] decimal LowAverage, [property: JsonPropertyName("highAverage")] decimal HighAverage, [property: JsonPropertyName("volume")] long Volume);
