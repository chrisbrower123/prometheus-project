using System.Net;
using System.Net.Http.Headers;

namespace StockData.Infrastructure.Tests.TestSupport;

public static class ResponseFactory
{
    public static HttpResponseMessage Json(HttpStatusCode status, string body, string contentType = "application/json") =>
        new(status) { Content = new StringContent(body, System.Text.Encoding.UTF8, contentType) };
}
