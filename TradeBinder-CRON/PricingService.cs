
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Text;
using TradeBinder_CRON.Models;
using TradeBinder_CRON.Models.ModelMetadata;

namespace TradeBinder_CRON
{
    public class PricingService : IHostedService, IDisposable
    {
        private Timer? _timer;
        private readonly DailyPriceData _dailyPriceData;
        private HttpClient _httpClient;
        private HtmlDocument _htmlDocument;
        private TradeBinderContext _TBContext;

        public PricingService(DailyPriceData priceData)
        {
            //Pull in pricing data
            _dailyPriceData = priceData;

            //Initialize http client
            _httpClient = new();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");

            //Initialize HTML parser
            _htmlDocument = new();

            _TBContext = new TradeBinderContext();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(UpdatePricingData, null, TimeSpan.Zero, TimeSpan.FromSeconds(3600));
            return Task.CompletedTask;
        }

        private void UpdatePricingData(object? state)
        {
            DateTime startTime = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("Start time: " + startTime.ToLongTimeString());

            //returns html of bulk data page
            //string response = CallUrl("https://scryfall.com/docs/api/bulk-data", _httpClient).Result;

            //return location of hosted JSON data
            //string dumpURL = GetAllCardsUrl(response);


            string readContents;

            using (StreamReader streamReader = new("", Encoding.UTF8))
            {
                readContents = streamReader.ReadToEnd();
            }

            //return JSON dump as JToken array
            List<Card> cards = JsonConvert.DeserializeObject<List<Card>>(
                readContents,
                new CardConverter());

            //Card tempCard = cards[0];
            //System.Diagnostics.Debug.WriteLine(tempCard.color);

            ////Add card data to database
            ////_TBContext.Card.Add(tempCard);



            //_TBContext.SaveChanges();
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
    }
}
