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
        public int? CollectionId { get; set; }
        public int? TradebinderId { get; set; }
        public int? WishlistId { get; set; }

        public virtual Wishlist? Wishlist { get; set; }
        public virtual Tradebinder? Tradebinder { get; set; }
        public virtual Collection? Collection { get; set; }
        public virtual ICollection<Report> Reports { get; set; }

    }
}
