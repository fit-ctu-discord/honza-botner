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

            new CounterMapper().Map(builder.Entity<Counter>());
        }

        public DbSet<Counter> Counters { get; set; }
        public DbSet<Verification> Verifications { get; set; }
    }
}
