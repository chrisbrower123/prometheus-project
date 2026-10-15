using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
namespace StockData.Api.ErrorHandling;
/// <summary>Maps domain exceptions to RFC 7807 responses.</summary>
public sealed class StockDataExceptionHandler : IExceptionHandler
{
    private readonly ILogger<StockDataExceptionHandler> logger;

    public StockDataExceptionHandler(ILogger<StockDataExceptionHandler> logger) => this.logger = logger;

    /// <inheritdoc />
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // TODO: map domain exceptions to documented status codes, Retry-After, and generic non-leaking ProblemDetails.
        _ = logger;
        throw new NotImplementedException();
    }
}
