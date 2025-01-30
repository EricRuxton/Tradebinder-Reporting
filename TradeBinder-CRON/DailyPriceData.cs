using System.Collections.Concurrent;
using TradeBinder_CRON.Models;

namespace TradeBinder_CRON;

public class DailyPriceData
{
    //Allow access to data from multiple threads at the same time
    public ConcurrentBag<Card> PriceData { get; set; } = new();

}
