using Microsoft.Extensions.DependencyInjection;
using StockData.Domain.Abstractions;
using StockData.Domain.Aggregation;
using StockData.Domain.Services;
namespace StockData.Domain.DependencyInjection;
/// <summary>Domain dependency injection registrations.</summary>
public static class DomainServiceCollectionExtensions
{
    /// <summary>Registers core services.</summary>
    public static IServiceCollection AddStockDataDomain(this IServiceCollection services)
    {
        services.AddSingleton<IDailyAggregator, DailyAggregator>();
        services.AddSingleton<IDailySummaryService, DailySummaryService>();
        services.AddSingleton(TimeProvider.System);
        return services;
    }
}
