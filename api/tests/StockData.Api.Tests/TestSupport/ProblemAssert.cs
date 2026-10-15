using System.Text.Json;
using System.Net;

namespace StockData.Api.Tests.TestSupport;

public static class ProblemAssert
{
    public static async Task<JsonDocument> Read(HttpResponseMessage response, HttpStatusCode expected)
    {
        Assert.Equal(expected, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }
}
