using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HonzaBotner.Database.Mappers
{
    internal sealed class RoleBindingMapper : IEntityMapper<RoleBinding>
    {
        public void Map(EntityTypeBuilder<RoleBinding> builder)
        {
            // Is having all columns as composite key good idea?
            builder.HasKey(rb => new {rb.Emoji, rb.ChannelId, rb.MessageId, rb.RoleId});
        }
    }
}
