using Microsoft.EntityFrameworkCore;

namespace TradeBinder_CRON.Models
{
    public class TradeBinderContext : DbContext
    {
        static readonly string connectionString = "Server=localhost; User ID=root; Password=root; Database=tradebinder; AllowLoadLocalInfile=true";

        public DbSet<Card> Card { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Card>().HasKey(c => c.id);

            modelBuilder.Entity<Card>().Property(c => c.id).ValueGeneratedOnAdd();

            modelBuilder.Entity<Card>().HasAlternateKey(c => c.scryfallId).HasName("tempUniqueIndex_Card_scryfallId");
        }
    }
}
