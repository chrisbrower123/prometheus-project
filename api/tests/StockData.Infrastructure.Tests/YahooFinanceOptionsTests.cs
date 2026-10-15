using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using StockData.Infrastructure.Providers.Yahoo;

namespace StockData.Infrastructure.Tests;

public sealed class YahooFinanceOptionsTests
{
    [Fact]
    public void Defaults_UserAgentIsBriefStringAndTimeoutIsTenSeconds()
    {
        var options = new YahooFinanceOptions();
        Assert.Contains("Mozilla/5.0", options.UserAgent);
        Assert.Equal(TimeSpan.FromSeconds(10), options.Timeout);
    }

    [Theory]
    [InlineData("https://query1.finance.yahoo.com", 1)]
    [InlineData("http://localhost:5000", 60)]
    public void Validate_ValidConfiguration_Passes(string baseUrl, int timeout)
    {
        var options = new YahooFinanceOptions { BaseUrl = baseUrl, Timeout = TimeSpan.FromSeconds(timeout) };
        Assert.True(Validator.TryValidateObject(options, new ValidationContext(options), new List<ValidationResult>(), true));
    }

    [Theory]
    [InlineData("", "Mozilla", 10)]
    [InlineData("/v8/finance", "Mozilla", 10)]
    [InlineData("https://example.com", "", 10)]
    [InlineData("https://example.com", "Mozilla", 0)]
    [InlineData("https://example.com", "Mozilla", -1)]
    [InlineData("https://example.com", "Mozilla", 61)]
    public void Validate_InvalidConfiguration_Fails(string baseUrl, string userAgent, int timeout)
    {
        var options = new YahooFinanceOptions { BaseUrl = baseUrl, UserAgent = userAgent, Timeout = TimeSpan.FromSeconds(timeout) };
        Assert.False(Validator.TryValidateObject(options, new ValidationContext(options), new List<ValidationResult>(), true));
    }

    [Fact]
    public void Bind_FromConfiguration_MapsValuesAndSectionName()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"{YahooFinanceOptions.SectionName}:BaseUrl"] = "https://example.com",
            [$"{YahooFinanceOptions.SectionName}:UserAgent"] = "agent",
            [$"{YahooFinanceOptions.SectionName}:Timeout"] = "00:00:10",
        }).Build();
        var options = configuration.GetSection(YahooFinanceOptions.SectionName).Get<YahooFinanceOptions>()!;
        Assert.Equal(YahooFinanceOptions.SectionName, "YahooFinance");
        Assert.Equal("https://example.com", options.BaseUrl);
        Assert.Equal(TimeSpan.FromSeconds(10), options.Timeout);
    }
}
