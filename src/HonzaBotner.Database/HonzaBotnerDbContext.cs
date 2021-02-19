using HonzaBotner.Database.Mappers;
using Microsoft.EntityFrameworkCore;

#nullable disable
namespace HonzaBotner.Database
{
    public class HonzaBotnerDbContext : DbContext
    {
        public HonzaBotnerDbContext(DbContextOptions<HonzaBotnerDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            new VerificationMapper().Map(builder.Entity<Verification>());
            new CountedEmojiMapper().Map(builder.Entity<CountedEmoji>());
        }

        public DbSet<Verification> Verifications { get; set; }
        public DbSet<CountedEmoji> CountedEmojis { get; set; }

        public DbSet<Zira> Ziras { get; set; }
    }
}
