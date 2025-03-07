using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using TradeBinder_CRON.Models;

namespace TradeBinder_CRON.Services
{
    public class DailyPricingService
    {
        private readonly DailyPriceData _dailyPriceData;
        private readonly HttpClient _httpClient;
        private readonly DailyReportingService _dailyReportingService;

        public DailyPricingService(DailyPriceData priceData)
        {
            //Pull in pricing data
            _dailyPriceData = priceData;

            //Initialize http client
            _httpClient = new();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");

            _dailyReportingService = new DailyReportingService(_dailyPriceData);
        }

        public async void UpdateGlobalPricingInfo()
        {
            DateTime startTime = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("Starting DailyPricingService");
            System.Diagnostics.Debug.WriteLine("Start time: " + startTime.ToLongTimeString());

            _dailyPriceData.PriceData.Clear();

            // Load existing card IDs from the database
            Dictionary<string, int> existingCardIDs;
            using (TradeBinderContext tbContext = new TradeBinderContext())
            {
                existingCardIDs = await tbContext.Card
                    .AsNoTracking()
                    .Select(c => new { c.Id, c.ScryfallId })
                    .ToDictionaryAsync(c => c.ScryfallId, c => c.Id);
            }

            const int batchSize = 1000;
            List<Task> processingTasks = new();

            JToken bulkInfoResponse = JToken.Parse(await CallUrl("https://api.scryfall.com/bulk-data/all-cards", _httpClient));

            // Download JSON from the URL
            using (var responseStream = await _httpClient.GetStreamAsync(bulkInfoResponse["download_uri"]!.Value<string>()!))
            using (var streamReader = new StreamReader(responseStream, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                JArray currentBatch = [];

                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.StartObject)
                    {
                        JObject cardJson = JObject.Load(jsonReader);
                        double? foilValue = ParseNullableDouble(cardJson["prices"]!["usd"]);
                        double? flatValue = ParseNullableDouble(cardJson["prices"]!["usd_foil"]);
                        double? etchedValue = ParseNullableDouble(cardJson["prices"]!["usd_etched"]);

                        // Filter and skip unwanted cards
                        if (cardJson["layout"]!.Value<string>()!.Equals("double_faced_token") ||
                            cardJson["layout"]!.Value<string>()!.Equals("art_series") ||
                            string.Join(",", cardJson["games"]!).Equals("mtgo") ||
                            string.Join(",", cardJson["games"]!).Equals("arena") ||
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
                            var batchToProcess = new JArray(currentBatch);
                            processingTasks.Add(Task.Run(() => ProcessBatchAsync(batchToProcess, existingCardIDs)));
                            currentBatch.Clear();
                        }
                    }
                }

                // Process any remaining cards
                if (currentBatch.Count > 0)
                {
                    var batchToProcess = new JArray(currentBatch);
                    processingTasks.Add(Task.Run(() => ProcessBatchAsync(batchToProcess, existingCardIDs)));
                    currentBatch.Clear();
                }
            }

            // Wait for all batch processing to complete
            await Task.WhenAll(processingTasks);

            System.Diagnostics.Debug.WriteLine("DailyPriceService end time: " + DateTime.Now.ToLongTimeString());
            System.Diagnostics.Debug.WriteLine($"Execution Time: {(DateTime.Now - startTime).TotalSeconds} seconds");

            await _dailyReportingService.GenerateReports();
        }

        private async Task ProcessBatchAsync(JArray batch, Dictionary<string, int> existingCardIDs)
        {
            List<Card> newCards = new();
            List<Card> existingCards = new();

            foreach (var cardJson in batch)
            {
                Card card = CreateCardFromJson(cardJson);

                if (existingCardIDs.TryGetValue(card.ScryfallId, out int existingId))
                {
                    card.Id = existingId;
                    existingCards.Add(card);
                }
                else
                {
                    newCards.Add(card);
                }
                _dailyPriceData.PriceData.Add(card);
            }

            // Batch insert/update
            using var tbContext = new TradeBinderContext();
            const int dbBatchSize = 1000;

            // Insert new cards in parallel
            for (int i = 0; i < newCards.Count; i += dbBatchSize)
            {
                var batchToInsert = newCards.Skip(i).Take(dbBatchSize).ToList();
                await tbContext.Card.AddRangeAsync(batchToInsert);
            }

            // Update existing cards in parallel
            for (int i = 0; i < existingCards.Count; i += dbBatchSize)
            {
                var batchToUpdate = existingCards.Skip(i).Take(dbBatchSize).ToList();
                tbContext.Card.UpdateRange(batchToUpdate);
            }
            await tbContext.SaveChangesAsync();
        }

        private Card CreateCardFromJson(JToken scryfallCard)
        {
            return new Card
            {
                ScryfallId = scryfallCard["id"]!.Value<string>()!,
                Name = scryfallCard["name"]!.Value<string>()!,
                CardType = scryfallCard["type_line"] != null ? scryfallCard["type_line"]!.Value<string>()! : scryfallCard["card_faces"]![0]!["type_line"]!.Value<string>()!,
                SetName = scryfallCard["set_name"]!.Value<string>()!,
                Color = scryfallCard["colors"] != null ? String.Join(",", scryfallCard["colors"]!) : String.Join(",", String.Join(",", scryfallCard["card_faces"]![0]!["colors"]!), String.Join(",", scryfallCard["card_faces"]![1]!["colors"]!).Distinct()),
                ColorIdentity = String.Join(",", scryfallCard["color_identity"]!),
                CMC = scryfallCard["cmc"] != null ? scryfallCard["cmc"]!.Value<int>()! : scryfallCard["card_faces"]![0]!["cmc"]!.Value<int>()!,
                FlatValue = ParseNullableDouble(scryfallCard["prices"]?["usd"]),
                FoilValue = ParseNullableDouble(scryfallCard["prices"]?["usd_foil"]),
                EtchedValue = ParseNullableDouble(scryfallCard["prices"]?["usd_etched"]),
                CardUri = scryfallCard["image_uris"] != null ? scryfallCard["image_uris"]!["small"]!.Value<string>()! : scryfallCard["card_faces"]![0]!["image_uris"]!["small"]!.Value<string>()!,
                ArtUri = scryfallCard["image_uris"] != null ? scryfallCard["image_uris"]!["art_crop"]!.Value<string>()! : scryfallCard["card_faces"]![0]!["image_uris"]!["art_crop"]!.Value<string>()!,
                SetCode = scryfallCard["set"]!.Value<string>()!,
                Finishes = String.Join(",", scryfallCard["finishes"]!),
                Language = scryfallCard["lang"]!.Value<string>()!,
                CollectorNumber = scryfallCard["collector_number"]!.Value<string>()!,
                Rarity = scryfallCard["rarity"]!.Value<string>()!
            };
        }

        private static async Task<string> CallUrl(string fullUrl, HttpClient httpClient)
        {
            var response = await httpClient.GetStringAsync(fullUrl);
            return response;
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
