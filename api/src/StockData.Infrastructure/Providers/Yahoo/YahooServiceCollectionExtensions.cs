using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StockData.Domain.Abstractions;
namespace StockData.Infrastructure.Providers.Yahoo;
/// <summary>Yahoo Finance dependency injection registrations.</summary>
public static class YahooServiceCollectionExtensions
{
    /// <summary>Registers the Yahoo Finance provider.</summary>
    public static IServiceCollection AddYahooMarketData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<YahooFinanceOptions>().Bind(configuration.GetSection(YahooFinanceOptions.SectionName)).ValidateDataAnnotations().ValidateOnStart();
        services.AddHttpClient<IMarketDataProvider, YahooMarketDataProvider>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<YahooFinanceOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
            client.Timeout = options.Timeout;
        }).AddStandardResilienceHandler();
        // TODO: tune retry and timeout settings for Yahoo Finance.
        return services;
    }
}
