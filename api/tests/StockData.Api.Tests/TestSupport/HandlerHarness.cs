using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using StockData.Api.ErrorHandling;

namespace StockData.Api.Tests.TestSupport;

public sealed class CollectingLogger<T> : ILogger<T>
{
    public List<LogLevel> Levels { get; } = [];
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => Levels.Add(logLevel);
    private sealed class NullScope : IDisposable { public static readonly NullScope Instance = new(); public void Dispose() { } }
}

public static class HandlerHarness
{
    public static (DefaultHttpContext Context, MemoryStream Body, CollectingLogger<StockDataExceptionHandler> Logger, StockDataExceptionHandler Handler) Create()
    {
        var body = new MemoryStream();
        var context = new DefaultHttpContext { RequestServices = new ServiceCollection().BuildServiceProvider() };
        context.Response.Body = body;
        var logger = new CollectingLogger<StockDataExceptionHandler>();
        return (context, body, logger, new StockDataExceptionHandler(logger));
    }
}
