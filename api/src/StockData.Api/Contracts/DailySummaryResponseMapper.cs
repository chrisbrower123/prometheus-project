using StockData.Domain.Models;
namespace StockData.Api.Contracts;
/// <summary>Maps domain summaries to API responses.</summary>
public static class DailySummaryResponseMapper
{
    public static IReadOnlyList<DailySummaryResponse> ToResponse(this IReadOnlyList<DailySummary> summaries)
    {
        // TODO: map each summary with the single-item mapper.
        throw new NotImplementedException();
    }

    /// <summary>Maps one summary.</summary>
    public static DailySummaryResponse ToResponse(this DailySummary summary)
    {
        // TODO: round averages away from zero to four decimals and format the day as yyyy-MM-dd.
        throw new NotImplementedException();
    }
}
