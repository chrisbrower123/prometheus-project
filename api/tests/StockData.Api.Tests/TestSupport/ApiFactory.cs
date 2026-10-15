using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;

namespace StockData.Api.Tests.TestSupport;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public FakeMarketDataProvider Provider { get; } = new();
    public FakeTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 2, 15, 14, 30, 0, TimeSpan.Zero));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<StockData.Domain.Abstractions.IMarketDataProvider>();
            services.AddSingleton<StockData.Domain.Abstractions.IMarketDataProvider>(Provider);
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }
}
