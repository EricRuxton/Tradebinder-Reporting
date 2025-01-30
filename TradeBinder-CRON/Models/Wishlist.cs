using System.ComponentModel.DataAnnotations;

namespace TradeBinder_CRON.Models
{
    public partial class Wishlist
    {
        public Wishlist()
        {
            WishlistCards = new HashSet<WishlistCard>();
        }

        [Key]
        public int Id { get; set; }
        public bool Public { get; set; }
        public required int UserId { get; set; }
        public required virtual User User { get; set; }
        public virtual ICollection<WishlistCard> WishlistCards { get; set; }
    }
}
