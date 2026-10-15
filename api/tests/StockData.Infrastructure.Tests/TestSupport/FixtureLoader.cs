using System.Reflection;
using System.Text.Json;
using StockData.Infrastructure.Providers.Yahoo;
using StockData.Infrastructure.Providers.Yahoo.Dto;

namespace StockData.Infrastructure.Tests.TestSupport;

public static class FixtureLoader
{
    public static string Load(string name)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"StockData.Infrastructure.Tests.Fixtures.{name}.json")
            ?? throw new InvalidOperationException($"Fixture not found: {name}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    internal static YahooChartResponse LoadResponse(string name) => JsonSerializer.Deserialize<YahooChartResponse>(Load(name), YahooJson.Options)!;
}
