using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HonzaBotner.Database.Mappers
{
    internal sealed class CountedEmojiMapper : IEntityMapper<CountedEmoji>
    {
        public void Map(EntityTypeBuilder<CountedEmoji> builder)
        {
            builder.HasKey(v => v.Id);
        }
    }
}
