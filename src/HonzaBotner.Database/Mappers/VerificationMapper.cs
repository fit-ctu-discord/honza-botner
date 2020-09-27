using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HonzaBotner.Database.Mappers
{
    internal sealed class VerificationMapper : IEntityMapper<Verification>
    {
        public void Map(EntityTypeBuilder<Verification> builder)
        {
            builder.HasKey(v => v.VerificationId);
            builder.HasIndex(v => new {v.GuildId, v.UserId})
                .IsUnique();
            builder.HasIndex(v => v.CvutUsername)
                .IsUnique(); // By default it is "IS UNIQUE IF NOT NULL"
        }
    }
}
