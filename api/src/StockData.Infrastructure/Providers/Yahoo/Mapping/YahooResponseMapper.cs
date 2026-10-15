using StockData.Domain.Exceptions;
using StockData.Domain.Models;
using StockData.Infrastructure.Providers.Yahoo.Dto;
namespace StockData.Infrastructure.Providers.Yahoo.Mapping;
internal static class YahooResponseMapper
{
    internal static IntradaySeries Map(Symbol symbol, YahooChartResponse response)
    {
        // TODO: resolve timezone, zip timestamps with quote arrays, skip null fields, and translate Not Found errors.
        throw new NotImplementedException();
    }
}
