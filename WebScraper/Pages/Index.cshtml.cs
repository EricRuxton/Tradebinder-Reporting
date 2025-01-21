using Microsoft.AspNetCore.Mvc.RazorPages;

using HtmlAgilityPack;
using System.Text;
using Newtonsoft.Json.Linq;
using MySqlConnector;


namespace WebScraper.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async void OnGet()
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
            using var connection = new MySqlConnection("Server=localhost;User ID=root;Password=root;Database=tradebinder");

            await connection.OpenAsync();
           
            using var openCommand = new MySqlCommand("USE tradebinder", connection);
            using var openCommandReader = await openCommand.ExecuteReaderAsync();
            openCommandReader.Close();

            foreach (JToken item in importedCards)
            {
                string checkExistsCommand = $"SELECT * FROM tradebinder.card WHERE scryfallId = '{item["id"]}';";
                using var checkExistsCommandReader = await new MySqlCommand(checkExistsCommand, connection).ExecuteReaderAsync();

                string commandString;

                if (checkExistsCommandReader.HasRows)
                {
                    string insertCommandString = $"INSERT INTO card " +
                        $"(scryfallId, name, cardType, setName, setCode, flatValue, foilValue, color, colorIdentity, " +
                        $"cmc, rarity, cardUri, artUri, finishes, language, collectorNumber) " +
                        $"VALUES('{item["id"]}', '{item["name"]}', '{item["type_line"]}', '{item["set_name"]}', " +
                        $"'{item["set"]}', '{item["prices"]["usd"]}','{item["prices"]["usd_foil"]}', '{item["colors"]}', '{item["color_identity"]}', " +
                        $"'{item["cmc"]}', '{item["rarity"]}', '{item["uri"]}', '{item["image_uris"]["large"]}', '{item["finishes"]}', '{item["lang"]}', " +
                        $"'{item["collector_number"]}');";

                    commandString = insertCommandString;
                }
                else
                {
                    string updateCommandString = $"UPDATE card " +
                        $"SET name = '{item["name"]}', cardType = '{item["type_line"]}'," +
                        $"setName = '{item["set_name"]}', setCode = '{item["set"]}', flatValue = '{item["prices"]["usd"]}'," +
                        $"foilValue = '{item["prices"]["usd_foil"]}', color = '{item["colors"]}', colorIdentity = '{item["color_identity"]}', " +
                        $"cmc = '{item["cmc"]}', rarity = '{item["rarity"]}', cardUri = '{item["uri"]}', artUri = '{item["image_uris"]["large"]}', " +
                        $" finishes = '{item["finishes"]}', language = '{item["lang"]}', collectorNumber = '{item["collector_number"]}' " +
                        $"WHERE scryfallId = '{item["id"]}';";

                    commandString = updateCommandString;
                }
                checkExistsCommandReader.Close();
                string escapedSqlCommandString =  MySqlHelper.EscapeString(commandString);
                using var reader = await new MySqlCommand(escapedSqlCommandString, connection).ExecuteReaderAsync();
                reader.Close();
            }
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
