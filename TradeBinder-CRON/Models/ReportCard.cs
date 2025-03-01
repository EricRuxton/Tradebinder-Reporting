using System.ComponentModel.DataAnnotations;

namespace TradeBinder_CRON.Models
{
    public partial class ReportCard
    {
        [Key]
        public int Id { get; set; }
        public required double ValuePerCard { get; set; }
        public required string Finish { get; set; }
        public int? Quantity { get; set; }
        public int? ReportId { get; set; }
        public required int CardId { get; set; }

        public virtual Report? Report { get; set; }
        public virtual Card? Card { get; set; }
    }
}
