using System.ComponentModel.DataAnnotations;

namespace TradeBinder_CRON.Models
{
    public partial class Collection
    {
        public Collection()
        {
            CollectionCards = new HashSet<CollectionCard>();
        }

        [Key]
        public int Id { get; set; }

        public required bool Public { get; set; }
        public required int UserId { get; set; }

        public required virtual User User { get; set; }
        public virtual ICollection<CollectionCard> CollectionCards { get; set; }

    }
}
