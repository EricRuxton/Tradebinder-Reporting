using System.ComponentModel.DataAnnotations;

namespace TradeBinder_CRON.Models
{
    public partial class ReportCard
    {
        [Key]
        public int Id { get; set; }
        public double ValuePerCard { get; set; }
        public string Finish { get; set; }
        public int? Quantity { get; set; }
        public int? ReportId { get; set; }
        public int CardId { get; set; }

        public virtual Report? Report { get; set; }
        public virtual Card Card { get; set; }

        public ReportCard(double valuePerCard, string finish, int cardId, Card priceCard)
        {
            ValuePerCard = valuePerCard;
            Finish = finish;
            CardId = cardId;
            Card = priceCard;
        }

        public ReportCard(ReportCard existingReportCard, int quantity)
        {
            Card = existingReportCard.Card;
            CardId = existingReportCard.CardId;
            ValuePerCard = existingReportCard.ValuePerCard;
            Finish = existingReportCard.Finish;
            Quantity = quantity;
        }
    }
}
