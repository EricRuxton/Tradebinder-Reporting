using Microsoft.EntityFrameworkCore;

namespace TradeBinder_CRON.Models
{
    public class TradeBinderContext : DbContext
    {
        static readonly string connectionString = "Server=localhost; User ID=root; Password=root; Database=Tradebinder; AllowLoadLocalInfile=true";


        public DbSet<Card> Card { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Collection> Collection { get; set; }
        public DbSet<Wishlist> Wishlist { get; set; }
        public DbSet<Tradebinder> Tradebinder { get; set; }
        public DbSet<CollectionCard> CollectionCard { get; set; }
        public DbSet<WishlistCard> WishlistCard { get; set; }
        public DbSet<Report> Report { get; set; }
        public DbSet<ReportCard> ReportCard { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Card>(entity =>
            {
                entity.ToTable("card");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).ValueGeneratedOnAdd();
                entity.HasAlternateKey(c => c.ScryfallId).HasName("tempUniqueIndex_Card_scryfallId");
                entity.Property(c => c.Id).HasColumnName("id");
                entity.Property(c => c.ScryfallId).HasColumnName("scryfallId");
                entity.Property(c => c.Name).HasColumnName("name");
                entity.Property(c => c.CardType).HasColumnName("cardType");
                entity.Property(c => c.SetName).HasColumnName("setName");
                entity.Property(c => c.Color).HasColumnName("color");
                entity.Property(c => c.ColorIdentity).HasColumnName("colorIdentity");
                entity.Property(c => c.CMC).HasColumnName("cmc");
                entity.Property(c => c.FlatValue).HasColumnName("flatValue");
                entity.Property(c => c.FoilValue).HasColumnName("foilValue");
                entity.Property(c => c.EtchedValue).HasColumnName("etchedValue");
                entity.Property(c => c.CardUri).HasColumnName("cardUri");
                entity.Property(c => c.ArtUri).HasColumnName("artUri");
                entity.Property(c => c.SetCode).HasColumnName("setCode");
                entity.Property(c => c.Finishes).HasColumnName("finishes");
                entity.Property(c => c.Language).HasColumnName("language");
                entity.Property(c => c.CollectorNumber).HasColumnName("collectorNumber");
                entity.Property(c => c.Rarity).HasColumnName("rarity");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).ValueGeneratedOnAdd();
                entity.Property(u => u.Id).HasColumnName("id");
                entity.Property(u => u.Username).HasColumnName("username");
                entity.Property(u => u.Email).HasColumnName("email");
                entity.Property(u => u.Verified).HasColumnName("verified");
                entity.Property(u => u.CollectionId).HasColumnName("collectionId");
                entity.Property(u => u.TradebinderId).HasColumnName("tradebinderId");
                entity.Property(u => u.WishlistId).HasColumnName("wishlistId");
                entity.HasOne(u => u.Collection).WithOne(c => c.User).HasForeignKey<Collection>(c => c.UserId).HasConstraintName("REL_9202407bb87b8bbb76cf3c18b9");
                entity.HasOne(u => u.Wishlist).WithOne(c => c.User).HasForeignKey<Wishlist>(c => c.UserId).HasConstraintName("REL_9583a1a42eebde5b1c16ee166d");
                entity.HasOne(u => u.Tradebinder).WithOne(c => c.User).HasForeignKey<Tradebinder>(c => c.UserId).HasConstraintName("REL_ed2874a985d6c37cf1cad6bbf1");
            });

            modelBuilder.Entity<Collection>(entity =>
            {
                entity.ToTable("collection");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).ValueGeneratedOnAdd();
                entity.Property(c => c.Id).HasColumnName("id");
                entity.Property(c => c.UserId).HasColumnName("userId");
                entity.HasOne(c => c.User).WithOne(u => u.Collection).HasForeignKey<User>(u => u.CollectionId).HasConstraintName("REL_ca25eb01f75a85272300f33602");
            });

            modelBuilder.Entity<Wishlist>(entity =>
            {
                entity.ToTable("wishlist");
                entity.HasKey(w => w.Id);
                entity.Property(w => w.Id).ValueGeneratedOnAdd();
                entity.Property(w => w.Id).HasColumnName("id");
                entity.Property(w => w.UserId).HasColumnName("userId");
                entity.HasOne(w => w.User).WithOne(u => u.Wishlist).HasForeignKey<User>(u => u.WishlistId).HasConstraintName("REL_f6eeb74a295e2aad03b76b0ba8");
            });

            modelBuilder.Entity<Tradebinder>(entity =>
            {
                entity.ToTable("tradebinder");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id).ValueGeneratedOnAdd();
                entity.Property(t => t.Id).HasColumnName("id");
                entity.Property(t => t.UserId).HasColumnName("userId");
                entity.Property(t => t.Threshold).HasColumnName("threshold");
                entity.Property(t => t.Public).HasColumnName("public");
                entity.HasOne(t => t.User).WithOne(u => u.Tradebinder).HasForeignKey<User>(u => u.TradebinderId).HasConstraintName("REL_20b5a544f4cdaf5fff48981147");
            });

            modelBuilder.Entity<CollectionCard>(entity =>
            {
                entity.ToTable("collection_card");
                entity.HasKey(cc => cc.Id);
                entity.Property(cc => cc.Id).ValueGeneratedOnAdd();
                entity.Property(cc => cc.Id).HasColumnName("id");
                entity.Property(cc => cc.CollectionId).HasColumnName("collectionId");
                entity.Property(cc => cc.CardId).HasColumnName("cardId");
                entity.Property(cc => cc.Tradeable).HasColumnName("tradeable");
                entity.Property(cc => cc.Finish).HasColumnName("finish");
                entity.HasOne(cc => cc.Collection).WithMany(c => c.CollectionCards).HasForeignKey(cc => cc.CollectionId).HasConstraintName("FK_f626e7047eee12f78d0b0b0a9b3");
                entity.HasOne(cc => cc.Card).WithMany(c => c.CollectionCards).HasForeignKey(cc => cc.CardId).HasConstraintName("FK_b09001b379829c0ceed0b59e299");
            });

            modelBuilder.Entity<WishlistCard>(entity =>
            {
                entity.ToTable("wishlist_card");
                entity.HasKey(wc => wc.Id);
                entity.Property(wc => wc.Id).ValueGeneratedOnAdd();
                entity.Property(wc => wc.Id).HasColumnName("id");
                entity.Property(wc => wc.WishlistId).HasColumnName("wishlistId");
                entity.Property(wc => wc.CardId).HasColumnName("cardId");
                entity.Property(wc => wc.Tradeable).HasColumnName("tradeable");
                entity.Property(wc => wc.Foil).HasColumnName("foil");
                entity.HasOne(wc => wc.Wishlist).WithMany(w => w.WishlistCards).HasForeignKey(wc => wc.WishlistId).HasConstraintName("FK_b42a2f4b513d7e8fec2d9779afe");
                entity.HasOne(wc => wc.Card).WithMany(c => c.WishlistCards).HasForeignKey(wc => wc.CardId).HasConstraintName("FK_9788f6c09c78435e8a1cb75a663");
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.ToTable("report");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Id).ValueGeneratedOnAdd();
                entity.Property(r => r.Id).HasColumnName("id");
                entity.Property(r => r.Value).HasColumnName("value");
                entity.Property(r => r.UserId).HasColumnName("userId");
                entity.Property(r => r.CreatedDate).HasColumnName("createdDate");
                entity.Property(r => r.CollectionSize).HasColumnName("collectionSize");
                entity.Property(r => r.IsDaily).HasColumnName("isDaily");
                entity.Property(r => r.IsWeekly).HasColumnName("isWeekly");
                entity.Property(r => r.IsMonthly).HasColumnName("isMonthly");
                entity.Property(r => r.Type).HasColumnName("type");
                entity.HasOne(r => r.User).WithMany(u => u.Reports).HasForeignKey(r => r.UserId).HasConstraintName("REL_e347c56b008c2057c9887e230a");
            });

            modelBuilder.Entity<ReportCard>(entity =>
            {
                entity.ToTable("report_card");
                entity.HasKey(rc => rc.Id);
                entity.Property(rc => rc.Id).ValueGeneratedOnAdd();
                entity.Property(rc => rc.Id).HasColumnName("id");
                entity.Property(rc => rc.ValuePerCard).HasColumnName("valuePerCard");
                entity.Property(rc => rc.Finish).HasColumnName("finish");
                entity.Property(rc => rc.Quantity).HasColumnName("quantity");
                entity.Property(rc => rc.ReportId).HasColumnName("reportId");
                entity.Property(rc => rc.CardId).HasColumnName("cardId");
                entity.HasOne(rc => rc.Card).WithMany(c => c.ReportCards).HasForeignKey(r => r.CardId).HasConstraintName("FK_0e6a086350c6517abdbc9ee966c");
                entity.HasOne(rc => rc.Report).WithMany(r => r.ReportCards).HasForeignKey(r => r.ReportId).HasConstraintName("FK_a3865c6406cc603417bd644d977");
            });
        }
    }
}
