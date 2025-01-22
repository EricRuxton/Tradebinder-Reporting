using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TradeBinder_CRON.Models.ModelMetadata
{
    public class CardConverter : JsonConverter<List<Card>>
    {
        public override List<Card>? ReadJson(JsonReader reader, Type objectType, List<Card>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            List<Card> cards = [];
            // Parse the JSON object
            var jsonArray = JArray.Load(reader);

            foreach (var sfCard in jsonArray)
            {
                if (
                    sfCard["layout"]?.Value<string>() != "double_faced_token" &&
                    sfCard["layout"]?.Value<string>() != "art_series")
                {

                    string color = "TEST";

                    if (sfCard["colors"] != null)
                    {
                        color = String.Join(",", sfCard["colors"]);
                    }
                    else if (sfCard["card_faces"] != null)
                    {
                        color = String.Join(",", [
                            String.Join(",", sfCard["card_faces"][0]["colors"]),
                            String.Join(",", sfCard["card_faces"][1]["colors"])
                            ]);
                    }
                    if (color == "TEST") break;

                    string colorIdentity = String.Join(",", sfCard["color_identity"]);
                    string artUri = sfCard["image_uris"] != null ? sfCard["image_uris"]["art_crop"]?.Value<string>() : sfCard["card_faces"][0]["image_uris"]["art_crop"]?.Value<string>();

                }



                //cards.Add(new Card(color));
            }
            return cards;
        }

        public override void WriteJson(JsonWriter writer, List<Card>? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
