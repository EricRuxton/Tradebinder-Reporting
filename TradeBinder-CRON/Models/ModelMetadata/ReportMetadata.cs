using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeBinder_CRON.Models
{
    [ModelMetadataType(typeof(ReportCardMetadata))]
    public partial class ReportCard
    {
        [NotMapped]
        public bool Tradeable { get; set; }
    }

    public class ReportCardMetadata
    {
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
