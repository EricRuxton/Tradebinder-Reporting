using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
            _timer = new Timer(UpdatePricingData, null, TimeSpan.Zero, TimeSpan.FromMinutes(60));
            return Task.CompletedTask;
        }

        private async void UpdatePricingData(object? state)
        {
            DateTime startTime = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("Start time: " + startTime.ToLongTimeString());

            // Load existing card IDs from the database
            Dictionary<string, int> existingCardIDs;
            using (var tbContext = new TradeBinderContext())
            {
                existingCardIDs = await tbContext.Card
                    .AsNoTracking()
                    .Select(c => new { c.id, c.scryfallId })
                    .ToDictionaryAsync(c => c.scryfallId, c => c.id);
            }

            var newCards = new List<Card>();
            var existingCards = new List<Card>();

            //JToken bulkInfoResponse = JToken.Parse(await CallUrl("https://api.scryfall.com/bulk-data/all-cards", _httpClient));
            JToken bulkInfoResponse = JToken.Parse(await CallUrl("https://api.scryfall.com/bulk-data/oracle-cards", _httpClient));

            // Download JSON from the URL
            using (var responseStream = await _httpClient.GetStreamAsync(bulkInfoResponse["download_uri"]!.Value<string>()!))
            using (var streamReader = new StreamReader(responseStream, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                JArray currentBatch = new();
                var batchSize = 1000;

                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.StartObject)
                    {
                        var cardJson = JObject.Load(jsonReader);
                        double? foilValue = ParseNullableDouble(cardJson["prices"]!["usd"]);
                        double? flatValue = ParseNullableDouble(cardJson["prices"]!["usd_foil"]);
                        double? etchedValue = ParseNullableDouble(cardJson["prices"]!["usd_etched"]);

                        // Filter and skip unwanted cards
                        if (cardJson["layout"]?.Value<string>() == "double_faced_token" ||
                            cardJson["layout"]?.Value<string>() == "art_series" ||
                            string.Join(",", cardJson["games"]!) == "mtgo" ||
                            string.Join(",", cardJson["games"]!) == "arena" ||
                            (foilValue == null && flatValue == null && etchedValue == null)
                        )
                        {
                            continue;
                        }

                        // Add card to batch
                        currentBatch.Add(cardJson);

                        // Process batch when it reaches the batch size
                        if (currentBatch.Count >= batchSize)
                        {
                            ProcessBatch(currentBatch, existingCardIDs, newCards, existingCards);
                            currentBatch.Clear();
                        }
                    }
                }

                // Process any remaining cards
                if (currentBatch.Count > 0)
                {
                    ProcessBatch(currentBatch, existingCardIDs, newCards, existingCards);
                }
            }

            // Batch insert and update to the database
            using (var tbContext = new TradeBinderContext())
            {
                const int dbBatchSize = 1000;

                // Insert new cards in batches
                for (int i = 0; i < newCards.Count; i += dbBatchSize)
                {
                    var batch = newCards.Skip(i).Take(dbBatchSize).ToList();
                    await tbContext.Card.AddRangeAsync(batch.Distinct());
                    await tbContext.SaveChangesAsync();
                }

                // Update existing cards in batches
                for (int i = 0; i < existingCards.Count; i += dbBatchSize)
                {
                    var batch = existingCards.Skip(i).Take(dbBatchSize).ToList();
                    tbContext.Card.UpdateRange(batch.Distinct());
                    await tbContext.SaveChangesAsync();
                }
            }

            System.Diagnostics.Debug.WriteLine("End time: " + DateTime.Now.ToLongTimeString());
            System.Diagnostics.Debug.WriteLine($"Execution Time: {(DateTime.Now - startTime).TotalSeconds} seconds");
        }

        private void ProcessBatch(
            JArray batch,
            Dictionary<string, int> existingCardIDs,
            List<Card> newCards,
            List<Card> existingCards)
        {
            foreach (var cardJson in batch)
            {
                var card = CreateCardFromJson(cardJson);

                if (existingCardIDs.TryGetValue(card.scryfallId, out int existingId))
                {
                    card.id = existingId; // Set the existing ID for updates
                    existingCards.Add(card);
                }
                else
                {
                    newCards.Add(card);
                }
            }
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
                cardType = scryfallCard["type_line"] != null ? scryfallCard["type_line"]!.Value<string>()! : scryfallCard["card_faces"]![0]!["type_line"]!.Value<string>()!,
                setName = scryfallCard["set_name"]!.Value<string>()!,
                color = scryfallCard["colors"] != null ? String.Join(",", scryfallCard["colors"]!) : String.Join(",", String.Join(",", scryfallCard["card_faces"]![0]!["colors"]!), String.Join(",", scryfallCard["card_faces"]![1]!["colors"]!).Distinct()),
                colorIdentity = String.Join(",", scryfallCard["color_identity"]!),
                cmc = scryfallCard["cmc"] != null ? scryfallCard["cmc"]!.Value<int>()! : scryfallCard["card_faces"]![0]!["cmc"]!.Value<int>()!,
                flatValue = ParseNullableDouble(scryfallCard["prices"]?["usd"]),
                foilValue = ParseNullableDouble(scryfallCard["prices"]?["usd_foil"]),
                etchedValue = ParseNullableDouble(scryfallCard["prices"]?["usd_etched"]),
                cardUri = scryfallCard["image_uris"] != null ? scryfallCard["image_uris"]!["small"]!.Value<string>()! : scryfallCard["card_faces"]![0]!["image_uris"]!["small"]!.Value<string>()!,
                artUri = scryfallCard["image_uris"] != null ? scryfallCard["image_uris"]!["art_crop"]!.Value<string>()! : scryfallCard["card_faces"]![0]!["image_uris"]!["art_crop"]!.Value<string>()!,
                setCode = scryfallCard["set"]!.Value<string>()!,
                finishes = String.Join(",", scryfallCard["finishes"]!),
                language = scryfallCard["lang"]!.Value<string>()!,
                collectorNumber = scryfallCard["collector_number"]!.Value<string>()!,
                rarity = scryfallCard["rarity"]!.Value<string>()!
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
    }
}
