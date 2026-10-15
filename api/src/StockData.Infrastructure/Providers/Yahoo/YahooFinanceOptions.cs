using System.ComponentModel.DataAnnotations;
namespace StockData.Infrastructure.Providers.Yahoo;
/// <summary>Yahoo Finance client options.</summary>
public sealed class YahooFinanceOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "YahooFinance";
    /// <summary>Provider base URL.</summary>
    [Required, Url] public string BaseUrl { get; set; } = string.Empty;
    /// <summary>HTTP user agent.</summary>
    [Required] public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
    /// <summary>HTTP timeout.</summary>
    [Range(typeof(TimeSpan), "00:00:01", "00:05:00")] public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}
