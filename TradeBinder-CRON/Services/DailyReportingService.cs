using Microsoft.EntityFrameworkCore;
using TradeBinder_CRON.Models;

namespace TradeBinder_CRON.Services
{
    public class DailyReportingService
    {
        public async Task<Collection[]> GenerateReports(DailyPriceData priceData)
        {
            TradeBinderContext dbContext = new TradeBinderContext();
            Collection[] collections = await dbContext.Collection
                .Include(c => c.User)
                .Include(c => c.CollectionCards)
                .ThenInclude(cc => cc.Card)
                .ToArrayAsync();
            return collections;
        }
    }
}
