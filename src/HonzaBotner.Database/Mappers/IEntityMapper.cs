using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HonzaBotner.Database.Mappers
{
    internal interface IEntityMapper<TEntity> where TEntity : class
    {
        void Map(EntityTypeBuilder<TEntity> builder);
    }
}
