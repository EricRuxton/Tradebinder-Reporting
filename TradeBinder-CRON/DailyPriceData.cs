using System.Collections.Concurrent;

namespace TradeBinder_CRON;

public class DailyPriceData
{
    //Allow access to data from multiple threads at the same time
    public ConcurrentBag<string> PriceData { get; set; } = new();
}
