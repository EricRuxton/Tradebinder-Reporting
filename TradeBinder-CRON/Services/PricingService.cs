using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Newtonsoft.Json.Linq;
using System.Text;
using TradeBinder_CRON.Models;

namespace TradeBinder_CRON.Services
{
    public class PricingService : IHostedService, IDisposable
    {
        private Timer? _timer;
        private readonly DailyPriceData _dailyPriceData;
        private HttpClient _httpClient;
        private HtmlDocument _htmlDocument;
        private TradeBinderContext _tbContext;

        const string UPSERT_SQL = @"
        INSERT INTO Card (scryfallId, name, cardType, setName, color, colorIdentity, cmc, flatValue, foilValue, etchedValue, cardUri, artUri, setCode, finishes, language, collectorNumber, rarity)
        VALUES (@scryfallId, @name, @cardType, @setName, @color, @colorIdentity, @cmc, @flatValue, @foilValue, @etchedValue, @cardUri, @artUri, @setCode, @finishes, @language, @collectorNumber, @rarity)
        ON DUPLICATE KEY UPDATE
            flatValue = VALUES(flatValue),
            foilValue = VALUES(foilValue),
            etchedValue = VALUES(etchedValue);";

        public PricingService(DailyPriceData priceData)
        {
            //Pull in pricing data
            _dailyPriceData = priceData;

            //Initialize http client
            _httpClient = new();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");

            //Initialize HTML parser
            _htmlDocument = new();

            _tbContext = new();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(UpdatePricingData, null, TimeSpan.Zero, TimeSpan.FromSeconds(3600));
            return Task.CompletedTask;
        }

        private async void UpdatePricingData(object? state)
        {
            DateTime startTime = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("Start time: " + startTime.ToLongTimeString());
            var existingCardIDs = _tbContext.Card.Select(c => new { c.id, c.scryfallId });

            //returns html of bulk data page
            //string response = CallUrl("https://scryfall.com/docs/api/bulk-data", _httpClient).Result;

            //return location of hosted JSON data
            //string dumpURL = GetAllCardsUrl(response);


            string readContents;
            //string readContents =  JArray.Parse(CallUrl(dumpURL, _httpClient).Result

            using (StreamReader streamReader = new("oracle-cards-20250121220522.json", Encoding.UTF8))
            {
                readContents = streamReader.ReadToEnd();
            }

            IEnumerable<JToken> scryfallCards = JArray.Parse(readContents)
                                                .Where(c =>
                                                    c["layout"]?.Value<string>() != "double_faced_token" &&
                                                    c["layout"]?.Value<string>() != "art_series" &&
                                                    string.Join(",", c["games"]!) != "mtgo" &&
                                                    string.Join(",", c["games"]!) != "arena"
                                                    );

            System.Diagnostics.Debug.WriteLine(scryfallCards.Count());
            // Process each chunk in parallel
            //Split data into chunks(e.g., batches of 10,000 entries)
            // var tasks = chunks.Select(async chunk =>
            // {
            //     TradeBinderContext _TBContext = new();

            //     foreach (var scryfallCard in chunk)
            //     {
            //         // Normalize and map JSON data to entity
            //         await InsertOrUpdateWithRawSqlAsync(CreateCardFromJson(scryfallCard!), _TBContext);
            //     }

            //     // Save entities to the database
            //     //await _TBContext.SaveChangesAsync();
            // });

            // //Wait for all tasks to complete

            //await Task.WhenAll(tasks);

            List<Card> cards = [];
            foreach (JToken scryfallCard in scryfallCards.GroupBy(sfc => sfc["id"]).Select(sfc => sfc.First()))
            {
                cards.Add(CreateCardFromJson(scryfallCard));
            }



            IEnumerable<Card> existingCards = cards.Where(c => existingCardIDs.Select(eci => eci.scryfallId).Contains(c.scryfallId)).Select(c => { c.id = existingCardIDs.First(eci => eci.scryfallId == c.scryfallId).id; return c; });
            IEnumerable<Card> newCards = cards.Where(c => !existingCardIDs.Select(eci => eci.scryfallId).Contains(c.scryfallId));

            System.Diagnostics.Debug.WriteLine(existingCards.Count());
            System.Diagnostics.Debug.WriteLine(newCards.Count());

            //await _tbContext.Card.AddRangeAsync(newCards);
            _tbContext.Card.UpdateRange(existingCards);

            await _tbContext.SaveChangesAsync();

            System.Diagnostics.Debug.WriteLine("End time: " + DateTime.Now.ToLongTimeString());
            System.Diagnostics.Debug.WriteLine("Execution Time: " + (DateTime.Now - startTime).TotalSeconds);

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private static async Task<string> CallUrl(string fullUrl, HttpClient httpClient)
        {
            var response = await httpClient.GetStringAsync(fullUrl);
            return response;
        }
        private string GetAllCardsUrl(string html)
        {
            _htmlDocument.LoadHtml(html);

            return _htmlDocument.DocumentNode.Descendants("a")
                .Where(node => node.GetAttributeValue("href", "").Contains("oracle-cards"))
                .First().GetAttributeValue("href", "");

        }

        private Card CreateCardFromJson(JToken scryfallCard)
        {
            return new Card
            {
                scryfallId = scryfallCard["id"]!.Value<string>()!,
                name = scryfallCard["name"]!.Value<string>()!,
                cardType = scryfallCard["type_line"]!.Value<string>()!,
                setName = scryfallCard["set_name"]!.Value<string>()!,
                color = scryfallCard["colors"] != null ?
                    String.Join(",", scryfallCard["colors"]!) :
                    (scryfallCard["card_faces"] != null ?
                        String.Join(",", new[]
                        {
                String.Join(",", scryfallCard["card_faces"]![0]!["colors"] ?? new JArray()),
                String.Join(",", scryfallCard["card_faces"]![1]!["colors"] ?? new JArray())
                        }) :
                        string.Empty),
                colorIdentity = String.Join(",", scryfallCard["color_identity"]!),
                cmc = scryfallCard["cmc"]!.Value<int>()!,
                flatValue = ParseNullableDouble(scryfallCard["prices"]?["usd"]),
                foilValue = ParseNullableDouble(scryfallCard["prices"]?["usd_foil"]),
                etchedValue = ParseNullableDouble(scryfallCard["prices"]?["usd_etched"]),
                cardUri = scryfallCard["image_uris"] != null ?
                    scryfallCard["image_uris"]!["small"]!.Value<string>()! :
                    scryfallCard["card_faces"]![0]!["image_uris"]!["small"]!.Value<string>()!,
                artUri = scryfallCard["image_uris"] != null ?
                    scryfallCard["image_uris"]!["art_crop"]!.Value<string>()! :
                    scryfallCard["card_faces"]![0]!["image_uris"]!["art_crop"]!.Value<string>()!,
                setCode = scryfallCard["set"]!.Value<string>()!,
                finishes = String.Join(",", scryfallCard["finishes"]!),
                language = scryfallCard["lang"]!.Value<string>()!,
                collectorNumber = scryfallCard["collector_number"]!.Value<string>()!,
                rarity = scryfallCard["rarity"]!.Value<string>()!,
            };
        }

        private static double? ParseNullableDouble(JToken? token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;

            if (double.TryParse(token.ToString(), out var result))
                return result;
            return null;
        }

        public async Task InsertOrUpdateWithRawSqlAsync(Card card, TradeBinderContext TBContext)
        {
            await TBContext.Database.ExecuteSqlRawAsync(UPSERT_SQL,
            [
                    new MySqlParameter("@scryfallId", card.scryfallId),
                    new MySqlParameter("@name", card.name),
                    new MySqlParameter("@cardType", card.cardType),
                    new MySqlParameter("@setName", card.setName),
                    new MySqlParameter("@color", card.color),
                    new MySqlParameter("@colorIdentity", card.colorIdentity),
                    new MySqlParameter("@cmc", card.cmc),
                    new MySqlParameter("@flatValue", card.flatValue),
                    new MySqlParameter("@foilValue", card.foilValue),
                    new MySqlParameter("@etchedValue", card.etchedValue),
                    new MySqlParameter("@cardUri", card.cardUri),
                    new MySqlParameter("@artUri", card.artUri),
                    new MySqlParameter("@setCode", card.setCode),
                    new MySqlParameter("@finishes", card.finishes),
                    new MySqlParameter("@language", card.language),
                    new MySqlParameter("@collectorNumber", card.collectorNumber),
                    new MySqlParameter("@rarity", card.rarity)
                ]);
        }
    }
}
