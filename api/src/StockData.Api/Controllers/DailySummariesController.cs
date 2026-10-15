using Microsoft.AspNetCore.Mvc;
using StockData.Api.Contracts;
using StockData.Domain.Abstractions;
using StockData.Domain.Models;
namespace StockData.Api.Controllers;
/// <summary>Daily stock summary endpoints.</summary>
[ApiController, Route("api/stocks/{symbol}/daily-summaries")]
public sealed class DailySummariesController(IDailySummaryService service) : ControllerBase
{
    /// <summary>Gets daily summaries for a symbol.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DailySummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDailySummaries(string symbol, CancellationToken cancellationToken)
    {
        var parsedSymbol = Symbol.Parse(symbol);
        var summaries = await service.GetDailySummariesAsync(parsedSymbol, cancellationToken);
        return Ok(summaries.ToResponse());
    }
}
