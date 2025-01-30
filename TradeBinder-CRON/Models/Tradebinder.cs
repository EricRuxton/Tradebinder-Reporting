using System.ComponentModel.DataAnnotations;

namespace TradeBinder_CRON.Models
{
    public partial class Tradebinder
    {
        [Key]
        public int Id { get; set; }
        public required decimal Threshold { get; set; }
        public required int UserId { get; set; }
        public required bool Public { get; set; }

        public required virtual User User { get; set; }
    }
}
