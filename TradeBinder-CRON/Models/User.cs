using System.ComponentModel.DataAnnotations;

namespace TradeBinder_CRON.Models
{
    public partial class User
    {

        public User()
        {
            Reports = new HashSet<Report>();
        }
        [Key]
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public bool Verified { get; set; }
        public required int CollectionId { get; set; }
        public required int TradebinderId { get; set; }
        public required int WishlistId { get; set; }

        public required virtual Wishlist Wishlist { get; set; }
        public required virtual Tradebinder Tradebinder { get; set; }
        public required virtual Collection Collection { get; set; }
        public virtual ICollection<Report> Reports { get; set; }

    }
}
