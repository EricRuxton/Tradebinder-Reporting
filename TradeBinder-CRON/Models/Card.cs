using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TradeBinder_CRON.Models
{
    [Index("ScryfallId", IsUnique = true)]
    public partial class Card
    {
        public Card()
        {
            CollectionCards = new HashSet<CollectionCard>();
            WishlistCards = new HashSet<WishlistCard>();
            ReportCards = new HashSet<ReportCard>();
        }

        [Key]
        public int Id { get; set; }

        public required string ScryfallId { get; set; }

        public required string Name { get; set; }

        public required string CardType { get; set; }

        public required string SetName { get; set; }

        public required string Color { get; set; }

        public required string ColorIdentity { get; set; }

        public int CMC { get; set; }

        public double? FlatValue { get; set; }

        public double? FoilValue { get; set; }

        public double? EtchedValue { get; set; }

        public required string CardUri { get; set; }

        public required string ArtUri { get; set; }

        public required string SetCode { get; set; }

        public required string Finishes { get; set; }

        public required string Language { get; set; }

        public required string CollectorNumber { get; set; }

        public required string Rarity { get; set; }

        public virtual ICollection<CollectionCard> CollectionCards { get; set; }

        public virtual ICollection<WishlistCard> WishlistCards { get; set; }

        public virtual ICollection<ReportCard> ReportCards { get; set; }
    }
}
