using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Globalization;
using StockData.Domain.Exceptions;
namespace StockData.Api.ErrorHandling;
/// <summary>Maps domain exceptions to RFC 7807 responses.</summary>
public sealed partial class StockDataExceptionHandler : IExceptionHandler
{
    private readonly ILogger<StockDataExceptionHandler> logger;

    public StockDataExceptionHandler(ILogger<StockDataExceptionHandler> logger) => this.logger = logger;

    /// <inheritdoc />
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            return ValueTask.FromResult(false);
        }

        var mapping = exception switch
        {
            InvalidSymbolException => new ErrorMapping(StatusCodes.Status400BadRequest, ProblemTypes.InvalidSymbol, "The symbol is invalid.", "The request contains an invalid symbol."),
            SymbolNotFoundException => new ErrorMapping(StatusCodes.Status404NotFound, ProblemTypes.SymbolNotFound, "Symbol not found.", "The requested symbol was not found."),
            NoDataAvailableException => new ErrorMapping(StatusCodes.Status404NotFound, ProblemTypes.NoDataAvailable, "No data available.", "No data is available for the requested symbol."),
            ProviderRateLimitedException => new ErrorMapping(StatusCodes.Status429TooManyRequests, ProblemTypes.RateLimited, "Request rate limited.", "The request was rate limited."),
            ProviderUnavailableException => new ErrorMapping(StatusCodes.Status502BadGateway, ProblemTypes.ProviderUnavailable, "Provider unavailable.", "The data provider is unavailable."),
            _ => new ErrorMapping(StatusCodes.Status500InternalServerError, ProblemTypes.Unexpected, "Request failed.", "The request could not be completed."),
        };

        if (exception is StockDataException)
        {
            LogDomainFailure(logger, exception);
        }
        else
        {
            LogUnexpectedFailure(logger, exception);
        }

        var problem = new ProblemDetails
        {
            Status = mapping.Status,
            Title = mapping.Title,
            Type = mapping.Type,
            Detail = mapping.Detail,
        };
        problem.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = mapping.Status;
        if (exception is ProviderRateLimitedException { RetryAfter: not null } rateLimited)
        {
            var seconds = Math.Max(1L, (long)Math.Ceiling(rateLimited.RetryAfter!.Value.TotalSeconds));
            httpContext.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
        }

        return new ValueTask<bool>(WriteResponseAsync(httpContext, problem, cancellationToken));
    }

    private static async Task<bool> WriteResponseAsync(HttpContext httpContext, ProblemDetails problem, CancellationToken cancellationToken)
    {
        await httpContext.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken);
        return true;
    }

    private sealed record ErrorMapping(int Status, string Type, string Title, string Detail);

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Stock data request failed.")]
    private static partial void LogDomainFailure(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Stock data request failed.")]
    private static partial void LogUnexpectedFailure(ILogger logger, Exception exception);
}
