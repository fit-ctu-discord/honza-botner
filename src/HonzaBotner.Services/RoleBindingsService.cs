using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HonzaBotner.Database;
using HonzaBotner.Services.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Services
{
    public class RoleBindingsService : IRoleBindingsService
    {
        private readonly HonzaBotnerDbContext _dbContext;
        private readonly ILogger<RoleBindingsService> _logger;

        public RoleBindingsService(HonzaBotnerDbContext dbContext, ILogger<RoleBindingsService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IList<ulong>> FindMappingAsync(ulong channelId, ulong messageId, string? emojiName = null)
        {
            bool ignoreEmoji = emojiName == null;

            return await _dbContext.RoleBindings
                .Where(z => z.ChannelId == channelId && z.MessageId == messageId &&
                            (z.Emoji == emojiName || ignoreEmoji))
                .Select(z => z.RoleId)
                .ToListAsync();
        }

        public async Task AddBindingsAsync(ulong channelId, ulong messageId, string emoji, HashSet<ulong> roleIds)
        {
            List<RoleBinding> bindingsToAdd = new();

            foreach (ulong roleId in roleIds)
            {
                RoleBinding binding = new()
                {
                    ChannelId = channelId, MessageId = messageId, Emoji = emoji, RoleId = roleId
                };

                if (await _dbContext.RoleBindings.AnyAsync(db => db.Emoji == binding.Emoji &&
                                                                 db.ChannelId == binding.ChannelId
                                                                 && db.MessageId == binding.MessageId
                                                                 && db.RoleId == binding.RoleId))
                {
                    _logger.LogInformation("Binding for this combination already exists (roleId: {RoleId})", roleId);
                    continue;
                }

                bindingsToAdd.Add(binding);
            }

            await _dbContext.RoleBindings.AddRangeAsync(bindingsToAdd);
            await _dbContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<bool> RemoveBindingsAsync(ulong channelId, ulong messageId, string emoji,
            HashSet<ulong>? roleIds)
        {
            List<RoleBinding> bindingsToRemove;

            RoleBinding binding = new() {ChannelId = channelId, MessageId = messageId, Emoji = emoji};

            if (roleIds != null && roleIds.Any())
            {
                bindingsToRemove = new List<RoleBinding>();

                foreach (ulong roleId in roleIds)
                {
                    binding.RoleId = roleId;
                    RoleBinding toDelete = await _dbContext.RoleBindings.FirstOrDefaultAsync(db =>
                        db.Emoji == binding.Emoji && db.ChannelId == binding.ChannelId
                                                  && db.MessageId == binding.MessageId
                                                  && db.RoleId == binding.RoleId);

                    if (toDelete != null)
                    {
                        bindingsToRemove.Add(toDelete);
                    }
                }
            }
            else
            {
                bindingsToRemove = await _dbContext.RoleBindings
                    .Where(db => db.Emoji == binding.Emoji && db.ChannelId == binding.ChannelId
                                                           && db.MessageId == binding.MessageId)
                    .ToListAsync();
            }


            _dbContext.RoleBindings.RemoveRange(bindingsToRemove);
            await _dbContext.SaveChangesAsync();

            return await _dbContext.RoleBindings
                .AnyAsync(db => db.Emoji == binding.Emoji && db.ChannelId == binding.ChannelId
                                                          && db.MessageId == binding.MessageId);
        }
    }
}
