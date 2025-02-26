//using Microsoft.AspNetCore.Mvc;

//namespace TradeBinder_CRON.Models
//{
//    [ModelMetadataType(typeof(CollectionCardMetadata))]
//    public partial class CollectionCard
//    {
//        public double Value => (double)(Finish == "flat" ? Card.FlatValue : Finish == "foil" ? Card.FoilValue : Card.EtchedValue)!;
//    }

//    public class CollectionCardMetadata
//    {
//        public int Id { get; set; }
//        public required bool Tradeable { get; set; }
//        public required string Finish { get; set; }
//        public required int CollectionId { get; set; }
//        public required int CardId { get; set; }

//        public required virtual Collection Collection { get; set; }
//        public required virtual Card Card { get; set; }
//    }
//}
