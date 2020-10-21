using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HonzaBotner.Database.Mappers
{
    internal sealed class CounterMapper : IEntityMapper<Counter>
    {
        public void Map(EntityTypeBuilder<Counter> builder)
        {
            builder.HasKey(p => p.UserId);
        }
    }
}
