using Microsoft.AspNetCore.Mvc.RazorPages;

using HtmlAgilityPack;
using System.Text;
using Newtonsoft.Json.Linq;
//using MySql.Data.MySqlClient;

namespace WebScraper.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            string url = "https://scryfall.com/docs/api/bulk-data";
            string response = CallUrl(url).Result;

            string allCardsUrl = GetAllCardsUrl(response);
            
            //string allCardsJsonRaw = CallUrl(allCardsUrl).Result;
            //JToken importedCards = JToken.Parse(allCardsJsonRaw);
            JToken importedCards;

            string readContents;

            using (StreamReader streamReader = new("D:\\projects\\WebScraper\\oracle-cards-20250110220431.json", Encoding.UTF8))
            {
                readContents = streamReader.ReadToEnd();
                importedCards = JToken.Parse(readContents);
            }
            /*
                foreach (JToken item in importedCards)
                {
                    ["id"]
                    ["name"]
                    ["type_line"]
                    ["set_name"]
                    ["set"]
                    ["prices"]["usd"]
                    ["prices"]["usd_foil"]
                    ["colors"]
                    ["color_identity"]
                    ["cmc"]
                    ["rarity"]
                    ["uri"]
                    ["image_uris"]["large"]
                    ["finishes"]
                    ["lang"]
                    ["collector_number"]
                }
            */

        }

        private static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");
            var response = await client.GetStringAsync(fullUrl);
            return response;
        }
        private string GetAllCardsUrl(string html)
        {
            HtmlDocument htmlDoc = new();
            htmlDoc.LoadHtml(html);

            return htmlDoc.DocumentNode.Descendants("a")
                .Where(node => node.GetAttributeValue("href", "").Contains("oracle-cards"))
                .First().GetAttributeValue("href", "");

        }
    }
}
