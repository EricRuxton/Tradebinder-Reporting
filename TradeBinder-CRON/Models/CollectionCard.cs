namespace TradeBinder_CRON.Models
{
    public class CollectionCard
    {
        public int Id { get; set; }
        public bool Tradeable { get; set; }
        public bool Foil { get; set; }
        public required int CollectionId { get; set; }
        public required int CardId { get; set; }

        public required virtual Collection Collection { get; set; }
        public required virtual Card Card { get; set; }
    }
}
