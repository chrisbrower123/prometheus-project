using Microsoft.AspNetCore.Diagnostics;
namespace StockData.Api.ErrorHandling;
/// <summary>Maps domain exceptions to RFC 7807 responses.</summary>
public sealed class StockDataExceptionHandler : IExceptionHandler
{
    /// <inheritdoc />
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // TODO: map domain exceptions to documented status codes, Retry-After, and generic non-leaking ProblemDetails.
        throw new NotImplementedException();
    }
}
