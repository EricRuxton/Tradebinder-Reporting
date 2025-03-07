﻿namespace TradeBinder_CRON.Models
{
    public partial class CollectionCard
    {
        public int Id { get; set; }
        public required bool Tradeable { get; set; }
        public required string Finish { get; set; }
        public required int CollectionId { get; set; }
        public required int CardId { get; set; }

        public required virtual Collection Collection { get; set; }
        public required virtual Card Card { get; set; }
    }
}
