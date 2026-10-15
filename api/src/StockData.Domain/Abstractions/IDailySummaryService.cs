using StockData.Domain.Models;
namespace StockData.Domain.Abstractions;
/// <summary>Retrieves daily summaries.</summary>
public interface IDailySummaryService
{
    /// <summary>Gets summaries covering the last month.</summary>
    Task<IReadOnlyList<DailySummary>> GetDailySummariesAsync(Symbol symbol, CancellationToken cancellationToken);
}
