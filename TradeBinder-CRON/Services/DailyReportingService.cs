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
        public async Task<User[]> GenerateReports()
        {
            DateTime startTime = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("Starting DailyReportingService");
            System.Diagnostics.Debug.WriteLine("Start time: " + startTime.ToLongTimeString());
            TradeBinderContext dbContext = new();
            User[] users = await dbContext.User
                .Include(u => u.Collection)
                .Include(u => u.Collection!.CollectionCards)
                .Include(u => u.Tradebinder)
                .Where(u => u.Verified == true && u.Collection != null && u.Tradebinder != null)
                .ToArrayAsync();

            List<Report> reports = [];

            for (int userIndex = 0; userIndex < users.Length; userIndex++)
            {
                if (users[userIndex].Collection!.CollectionCards.Count > 0)
                {
                    List<ReportCard> userReportCards = [];
                    Report collectionReport = new()
                    {
                        CreatedDate = DateTime.Now.Date.ToShortDateString(),
                        Value = 0,
                        User = users[userIndex],
                        UserId = users[userIndex].Id,
                        CollectionSize = 0,
                        IsDaily = true,
                        IsWeekly = DateTime.Now.DayOfWeek == DayOfWeek.Sunday,
                        IsMonthly = DateTime.Now.Day == 1,
                        Type = "Collection"
                    };
                    Report tradebinderReport = new()
                    {
                        CreatedDate = DateTime.Now.Date.ToShortDateString(),
                        Value = 0,
                        User = users[userIndex],
                        UserId = users[userIndex].Id,
                        CollectionSize = 0,
                        IsDaily = true,
                        IsWeekly = DateTime.Now.DayOfWeek == DayOfWeek.Sunday,
                        IsMonthly = DateTime.Now.Day == 1,
                        Type = "Tradebinder"
                    };
                    foreach (CollectionCard collectionCard in users[userIndex].Collection!.CollectionCards)
                    {
                        CollectionCard tempCollectionCard = collectionCard;
                        Console.WriteLine(tempCollectionCard.CardId);

                        Card priceCard = _dailyPriceData.PriceData.First(dpc => dpc.Id == collectionCard.CardId);
                        //memoization
                        ReportCard? existingReportCard = _dailyReportCards.FirstOrDefault(rc => rc?.CardId == collectionCard.CardId && rc?.Finish == collectionCard.Finish, null);
                        if (existingReportCard == null)
                        {
                            existingReportCard = GenerateReportCard(priceCard, collectionCard);
                            _dailyReportCards.Add(existingReportCard);
                        }
                        ReportCard? userReportCard = userReportCards.FirstOrDefault(rc => rc?.CardId == existingReportCard.CardId && rc?.Finish == existingReportCard.Finish, null);
                        if (userReportCard == null)
                        {
                            userReportCards.Add(new() { ValuePerCard = existingReportCard.ValuePerCard, Finish = existingReportCard.Finish, Quantity = 1, CardId = existingReportCard.CardId, Tradeable = collectionCard.Tradeable });
                        }
                        else
                        {
                            userReportCard.Quantity++;
                        }
                        collectionReport.Value += existingReportCard.ValuePerCard;
                        collectionReport.CollectionSize++;

                        //logic for tradebinder report
                        if ((decimal)existingReportCard.ValuePerCard > users[userIndex].Tradebinder!.Threshold && collectionCard.Tradeable)
                        {
                            tradebinderReport.Value += existingReportCard.ValuePerCard;
                            tradebinderReport.CollectionSize++;
                        }
                    }
                    collectionReport.ReportCards = userReportCards;
                    tradebinderReport.ReportCards = userReportCards.Where(rc => (decimal)rc.ValuePerCard >= users[userIndex].Tradebinder!.Threshold && rc.Tradeable).ToList();
                    reports.Add(collectionReport);
                    reports.Add(tradebinderReport);
                }
            }
            if (reports.Count > 0)
            {
                await dbContext.AddRangeAsync(reports);
                await dbContext.SaveChangesAsync();
            }
            System.Diagnostics.Debug.WriteLine("DailyPriceService end time: " + DateTime.Now.ToLongTimeString());
            System.Diagnostics.Debug.WriteLine($"Execution Time: {(DateTime.Now - startTime).TotalSeconds} seconds");
            return users;
        }

        private static ReportCard GenerateReportCard(Card priceCard, CollectionCard collectionCard)
        {
            return new()
            {
                ValuePerCard = (double)(collectionCard.Finish == "flat" ? priceCard.FlatValue :
                   collectionCard.Finish == "foil" ? priceCard.FoilValue :
                   priceCard.EtchedValue)!,
                Finish = collectionCard.Finish,
                CardId = collectionCard.CardId,
            };
        }
    }
}
