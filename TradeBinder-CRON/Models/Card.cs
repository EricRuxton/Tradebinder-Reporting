using Newtonsoft.Json;

namespace TradeBinder_CRON.Models
{
    public class Card
    {
        [JsonIgnore]
        public int id { get; set; }

        [JsonProperty("id")]
        public string scryfallId { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("type_line")]
        public string cardType { get; set; }

        [JsonProperty("set_name")]
        public string setName { get; set; }

        [JsonIgnore]
        public string? color { get; set; }

        [JsonIgnore]
        public string colorIdentity { get; set; }

        [JsonProperty("cmc")]
        public string cmc { get; set; }

        [JsonIgnore]
        public double? flatValue { get; set; }

        [JsonIgnore]
        public double? foilValue { get; set; }

        [JsonIgnore]
        public string cardUri { get; set; }

        [JsonIgnore]
        public string artUri { get; set; }

        [JsonProperty("set")]
        public string setCode { get; set; }

        [JsonIgnore]
        public string finishes { get; set; }

        [JsonProperty("lang")]
        public string language { get; set; }

        [JsonProperty("collector_number")]
        public string collectorNumber { get; set; }

        [JsonProperty("rarity")]
        public string rarity { get; set; }

        public Card(string color, string artUri, string colorIdentity, string flatValue, string foilValue, string cardUri, string finishes)
        {
            this.color = color;
            this.artUri = artUri;
            this.colorIdentity = colorIdentity;
            this.flatValue = double.Parse(flatValue);
            this.foilValue = double.Parse(foilValue);
            this.cardUri = cardUri;
            this.finishes = finishes;
        }

    }
}
