using Microsoft.AspNetCore.Mvc;
using StockData.Api.Contracts;
using StockData.Domain.Abstractions;
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public Task<IActionResult> GetDailySummaries(string symbol, CancellationToken cancellationToken)
    {
        _ = service;
        _ = symbol;
        _ = cancellationToken;
        // TODO: parse the symbol, call the service, map summaries, and return 200 OK.
        throw new NotImplementedException();
    }
}
