using System.Globalization;
using StockData.Domain.Models;
namespace StockData.Api.Contracts;
/// <summary>Maps domain summaries to API responses.</summary>
public static class DailySummaryResponseMapper
{
    public static IReadOnlyList<DailySummaryResponse> ToResponse(this IReadOnlyList<DailySummary> summaries)
    {
        ArgumentNullException.ThrowIfNull(summaries);
        return summaries.Select(ToResponse).ToArray();
    }

    /// <summary>Maps one summary.</summary>
    public static DailySummaryResponse ToResponse(this DailySummary summary)
    {
        return new DailySummaryResponse(
            summary.Day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Math.Round(summary.LowAverage, 4, MidpointRounding.AwayFromZero),
            Math.Round(summary.HighAverage, 4, MidpointRounding.AwayFromZero),
            summary.Volume);
    }
}
