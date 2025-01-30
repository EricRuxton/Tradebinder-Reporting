namespace TradeBinder_CRON.Models
{
    public partial class WishlistCard
    {
        public int Id { get; set; }
        public bool Tradeable { get; set; }
        public bool Foil { get; set; }
        public required int WishlistId { get; set; }
        public required int CardId { get; set; }

        public required virtual Wishlist Wishlist { get; set; }
        public required virtual Card Card { get; set; }
    }
}
