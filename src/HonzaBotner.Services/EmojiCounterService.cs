using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using HonzaBotner.Database;
using HonzaBotner.Services.Contract;
using Microsoft.EntityFrameworkCore;
using CountedEmoji = HonzaBotner.Services.Contract.Dto.CountedEmoji;

namespace HonzaBotner.Services
{
    public class EmojiCounterService : IEmojiCounterService
    {
        private readonly HonzaBotnerDbContext _dbContext;

        public EmojiCounterService(HonzaBotnerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<CountedEmoji>> ListAsync() =>
            (await _dbContext.CountedEmojis.ToListAsync().ConfigureAwait(false))
            .Select(GetDto).ToImmutableList();

        public async Task IncrementAsync(ulong emojiId)
        {
            Database.CountedEmoji emoji = await _dbContext.CountedEmojis.FindAsync(emojiId);

            if (emoji == null)
            {
                emoji = new Database.CountedEmoji() {Id = emojiId};
                await _dbContext.CountedEmojis.AddAsync(emoji);
            }

            emoji.Times++;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DecrementAsync(ulong emojiId)
        {
            Database.CountedEmoji emoji = await _dbContext.CountedEmojis.FindAsync(emojiId);

            if (emoji == null) return;

            emoji.Times--;
            await _dbContext.SaveChangesAsync();
        }


        private static CountedEmoji GetDto(Database.CountedEmoji emoji) =>
            new(emoji.Id, emoji.Times, emoji.FirstUsedAt);
    }
}
