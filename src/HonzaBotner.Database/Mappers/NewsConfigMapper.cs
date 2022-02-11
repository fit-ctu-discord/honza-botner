using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HonzaBotner.Database.Mappers;

internal sealed class NewsConfigMapper : IEntityMapper<NewsConfig>
{
    public void Map(EntityTypeBuilder<NewsConfig> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.Name).IsUnique();

        builder.Ignore(c => c.Channels);

        builder.Property(c => c.Name).IsRequired();
        builder.Property(c => c.Source).IsRequired();
        builder.Property(c => c.ChannelsData).IsRequired();
        builder.Property(c => c.NewsProviderType).IsRequired();
        builder.Property(c => c.PublisherType).IsRequired();
    }
}
