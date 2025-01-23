﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TradeBinder_CRON.Models
{
    [Index("scryfallId", IsUnique = true)]
    public class Card
    {
        [Key]
        public int id { get; set; }

        public string scryfallId { get; set; }

        public string name { get; set; }

        public string cardType { get; set; }

        public string setName { get; set; }

        public string color { get; set; }

        public string colorIdentity { get; set; }

        public int cmc { get; set; }

        public double? flatValue { get; set; }

        public double? foilValue { get; set; }

        public double? etchedValue { get; set; }

        public string cardUri { get; set; }

        public string artUri { get; set; }

        public string setCode { get; set; }

        public string finishes { get; set; }

        public string language { get; set; }

        public string collectorNumber { get; set; }

        public string rarity { get; set; }

    }
}
