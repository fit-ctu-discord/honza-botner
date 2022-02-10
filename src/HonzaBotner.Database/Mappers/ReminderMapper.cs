using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HonzaBotner.Database.Mappers;

internal sealed class ReminderMapper : IEntityMapper<Reminder>
{
    public void Map(EntityTypeBuilder<Reminder> builder)
    {
        builder.HasKey(reminder => reminder.Id);
        builder.HasIndex(reminder => reminder.DateTime);
    }
}
