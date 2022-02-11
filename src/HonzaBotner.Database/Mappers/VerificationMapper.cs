using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HonzaBotner.Database.Mappers;

internal sealed class VerificationMapper : IEntityMapper<Verification>
{
    public void Map(EntityTypeBuilder<Verification> builder)
    {
        builder.HasKey(v => v.UserId);
        builder.HasIndex(v => v.AuthId)
            .IsUnique(); // By default it is "IS UNIQUE IF NOT NULL"
    }
}
