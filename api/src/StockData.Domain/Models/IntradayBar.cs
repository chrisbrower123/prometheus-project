namespace StockData.Domain.Models;

/// <summary>An immutable intraday market bar.</summary>
public sealed record IntradayBar
{
    private decimal high;
    private decimal low;
    private long volume;

    public DateTimeOffset Timestamp { get; init; }

    public decimal High
    {
        get => high;
        init
        {
            if (value < low) throw new ArgumentException("High cannot be lower than Low.");
            high = value;
        }
    }

    public decimal Low
    {
        get => low;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(Low));
            if (high < value) throw new ArgumentException("High cannot be lower than Low.");
            low = value;
        }
    }

    public long Volume
    {
        get => volume;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(Volume));
            volume = value;
        }
    }

    public IntradayBar(DateTimeOffset timestamp, decimal high, decimal low, long volume)
    {
        Timestamp = timestamp;
        High = high;
        Low = low;
        Volume = volume;
    }
}
