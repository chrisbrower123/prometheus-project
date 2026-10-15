using System.Net;

namespace StockData.Infrastructure.Tests.TestSupport;

public sealed class StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : HttpMessageHandler
{
    public List<(Uri? RequestUri, HttpMethod Method)> Requests { get; } = [];
    public List<CancellationToken> CancellationTokens { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add((request.RequestUri, request.Method));
        CancellationTokens.Add(cancellationToken);
        return await handler(request, cancellationToken);
    }
}
