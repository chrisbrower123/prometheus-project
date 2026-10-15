using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using System.Net;
using StockData.Api.Tests.TestSupport;

namespace StockData.Api.Tests;

public sealed class HostingTests
{
    [Fact]
    public async Task Health_Get_Returns200AndDoesNotCallProvider()
    {
        await using var factory = new ApiFactory();
        var response = await factory.CreateClient().GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); Assert.Empty(factory.Provider.Calls);
    }

    [Theory]
    [InlineData("Development", HttpStatusCode.OK)]
    [InlineData("Production", HttpStatusCode.NotFound)]
    public async Task Swagger_Json_IsServedInDevelopmentAndHiddenInProduction(string environment, HttpStatusCode status)
    {
        await using var factory = new ApiFactory().WithWebHostBuilder(builder => builder.UseSetting("environment", environment));
        Assert.Equal(status, (await factory.CreateClient().GetAsync("/swagger/v1/swagger.json")).StatusCode);
    }

    [Theory]
    [InlineData("http://localhost:5173", true)]
    [InlineData("https://evil.example", false)]
    [InlineData("", false)]
    public async Task Cors_Preflight_OnlyConfiguredOriginsGetHeaders(string origin, bool allowed)
    {
        await using var factory = new ApiFactory().WithWebHostBuilder(builder => builder.UseSetting("Cors:AllowedOrigins:0", "http://localhost:5173"));
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/stocks/TSLA/daily-summaries");
        request.Headers.Add("Origin", origin); request.Headers.Add("Access-Control-Request-Method", "GET");
        var response = await factory.CreateClient().SendAsync(request);
        Assert.Equal(allowed, response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Theory]
    [InlineData("/api/stocks/a/b/daily-summaries")]
    [InlineData("/nope")]
    public async Task UnroutablePath_Returns404ProblemJson(string path)
    {
        await using var factory = new ApiFactory();
        var response = await factory.CreateClient().GetAsync(path);
        var problem = await ProblemAssert.Read(response, HttpStatusCode.NotFound);
        Assert.NotEmpty(problem.RootElement.GetProperty("type").GetString()!);
    }

    [Fact]
    public async Task WrongMethod_Post_Returns405ProblemJson()
    {
        await using var factory = new ApiFactory();
        var response = await factory.CreateClient().PostAsync("/api/stocks/TSLA/daily-summaries", null);
        await ProblemAssert.Read(response, HttpStatusCode.MethodNotAllowed);
    }
}
