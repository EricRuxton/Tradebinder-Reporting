using Microsoft.EntityFrameworkCore;
using TradeBinder_CRON.Models;

namespace TradeBinder_CRON.Services
{
    public class DailyReportingService
    {
        DailyPriceData _dailyPriceData;
        public DailyReportingService(DailyPriceData priceData)
        {
            _dailyPriceData = priceData;
        }
        public async Task<Collection[]> GenerateReports(DailyPriceData priceData)
        {
            DateTime startTime = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("Starting DailyReportingService");
            System.Diagnostics.Debug.WriteLine("Start time: " + startTime.ToLongTimeString());
            TradeBinderContext dbContext = new TradeBinderContext();
            Collection[] collections = await dbContext.Collection
                .Include(c => c.User)
                .Include(c => c.CollectionCards)
                .ToArrayAsync();
            System.Diagnostics.Debug.WriteLine("DailyPriceService end time: " + DateTime.Now.ToLongTimeString());
            System.Diagnostics.Debug.WriteLine($"Execution Time: {(DateTime.Now - startTime).TotalSeconds} seconds");
            return collections;
        }
    }
}
