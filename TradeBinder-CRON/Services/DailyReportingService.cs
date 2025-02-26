using Microsoft.EntityFrameworkCore;
using TradeBinder_CRON.Models;

namespace TradeBinder_CRON.Services
{
    public class DailyReportingService
    {
        private DailyPriceData _dailyPriceData;
        private List<ReportCard> _dailyReportCards;
        public DailyReportingService(DailyPriceData priceData)
        {
            _dailyPriceData = priceData;
            _dailyReportCards = [];
        }
        public async Task<Collection[]> GenerateReports(DailyPriceData priceData)
        {
            DateTime startTime = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("Starting DailyReportingService");
            System.Diagnostics.Debug.WriteLine("Start time: " + startTime.ToLongTimeString());
            TradeBinderContext dbContext = new();
            Collection[] collections = await dbContext.Collection
                .Include(c => c.User)
                .Include(c => c.CollectionCards)
                .ToArrayAsync();

            for (int collectionIndex = 0; collectionIndex < collections.Length; collectionIndex++)
            {
                List<ReportCard> userReportCards = [];
                foreach (CollectionCard collectionCard in collections[collectionIndex].CollectionCards)
                {
                    Card priceCard = _dailyPriceData.PriceData.First(dpc => dpc.Id == collectionCard.CardId);
                    //memoization
                    ReportCard? existingReportCard = _dailyReportCards.FirstOrDefault(rc => rc?.CardId == collectionCard.CardId, null);
                    if (existingReportCard == null)
                    {
                        existingReportCard = GenerateReportCard(priceCard, collectionCard);
                        _dailyReportCards.Add(existingReportCard);
                    }
                    ReportCard? userReportCard = userReportCards.FirstOrDefault(rc => rc?.CardId == existingReportCard.CardId && rc?.Finish == existingReportCard.Finish, null);
                    if (userReportCard == null)
                    {
                        userReportCards.Add(new ReportCard(existingReportCard, 1));
                    }
                    else
                    {
                        userReportCard.Quantity++;
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine("DailyPriceService end time: " + DateTime.Now.ToLongTimeString());
            System.Diagnostics.Debug.WriteLine($"Execution Time: {(DateTime.Now - startTime).TotalSeconds} seconds");
            return collections;
        }

        private ReportCard GenerateReportCard(Card priceCard, CollectionCard collectionCard)
        {
            return new((double)(collectionCard.Finish == "flat" ? priceCard.FlatValue :
                   collectionCard.Finish == "foil" ? priceCard.FoilValue :
                   priceCard.EtchedValue)!,
                   collectionCard.Finish,
                   collectionCard.CardId,
                   priceCard
                   );
        }
    }
}
