using HonzaBotner.Database.Mappers;
using Microsoft.EntityFrameworkCore;

#nullable disable
namespace HonzaBotner.Database;

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
        new RoleBindingMapper().Map(builder.Entity<RoleBinding>());
        new WarningMapper().Map(builder.Entity<Warning>());
        new ReminderMapper().Map(builder.Entity<Reminder>());
    }

    public DbSet<Verification> Verifications { get; set; }
    public DbSet<CountedEmoji> CountedEmojis { get; set; }
    public DbSet<RoleBinding> RoleBindings { get; set; }
    public DbSet<Warning> Warnings { get; set; }
    public DbSet<Reminder> Reminders { get; set; }
    public DbSet<StandUpStat> StandUpStats { get; set; }

}
