using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HonzaBotner.Database.Mappers
{
    internal sealed class WarningMapper : IEntityMapper<Warning>
    {
        public void Map(EntityTypeBuilder<Warning> builder)
        {
            builder.HasKey(w => w.Id);
        }
    }
}
