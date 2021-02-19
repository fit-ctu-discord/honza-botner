using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HonzaBotner.Database;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.EntityFrameworkCore;

namespace HonzaBotner.Services
{
    public class ZiraService : IZiraService
    {
        private readonly HonzaBotnerDbContext _dbContext;

        public ZiraService(HonzaBotnerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IList<ulong>> FindMappingAsync(ulong channelId, ulong messageId, string emojiName)
        {
            return await _dbContext.Ziras
                .Where(z => z.ChannelId == channelId && z.MessageId == messageId && z.Emoji == emojiName)
                .Select(z => z.RoleId)
                .ToListAsync();
        }
    }
}
