using HonzaBotner.Database.Mappers;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

#nullable disable
namespace HonzaBotner.Database
{
    public class HonzaBotnerDbContext : IdentityDbContext
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
    }
}
