using System.ComponentModel.DataAnnotations;

namespace TradeBinder_CRON.Models
{
    public partial class Report
    {
        public Report()
        {
            ReportCards = new HashSet<ReportCard>();
        }

        [Key]
        public int Id { get; set; }

        public required double Value { get; set; }
        public required int UserId { get; set; }
        public required DateTime CreatedDate { get; set; }
        public required String Type { get; set; }

        public required virtual User User { get; set; }
        public virtual ICollection<ReportCard> ReportCards { get; set; }

    }
}
