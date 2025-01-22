using Microsoft.EntityFrameworkCore;

namespace TradeBinder_CRON.Models
{
    public class TradeBinderContext : DbContext
    {
        static readonly string connectionString = "Server=localhost; User ID=root; Password=root; Database=tradebinder";

        public DbSet<Card> Card { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
    }
}
