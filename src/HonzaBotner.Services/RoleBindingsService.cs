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

        public async Task<IList<ulong>> FindMappingAsync(ulong channelId, ulong messageId, string emojiName)
        {
            return await _dbContext.RoleBindings
                .Where(z => z.ChannelId == channelId && z.MessageId == messageId && z.Emoji == emojiName)
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

                if (await _dbContext.RoleBindings.AnyAsync(r => Same(r, binding, true)))
                {
                    _logger.LogInformation("Binding for this combination already exists (roleId: {0})", roleId);
                    continue;
                }

                bindingsToAdd.Add(binding);
            }

            await _dbContext.RoleBindings.AddRangeAsync(bindingsToAdd);
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveBindingsAsync(ulong channelId, ulong messageId, string emoji, HashSet<ulong>? roleIds)
        {
            List<RoleBinding> bindingsToRemove;

            RoleBinding binding = new() {ChannelId = channelId, MessageId = messageId, Emoji = emoji};

            if (roleIds == null)
            {
                bindingsToRemove =  await _dbContext.RoleBindings
                    .Where(r => Same(r, binding, false))
                    .ToListAsync();
            }
            else
            {
                bindingsToRemove = new List<RoleBinding>();

                foreach (ulong roleId in roleIds)
                {
                    binding.RoleId = roleId;
                    RoleBinding toDelete = await _dbContext.RoleBindings.FirstOrDefaultAsync(r => Same(r, binding, true));

                    if (toDelete != null)
                    {
                        bindingsToRemove.Add(toDelete);
                    }
                }
            }

            _dbContext.RoleBindings.RemoveRange(bindingsToRemove);
            await _dbContext.RoleBindings.SingleOrDefaultAsync();
        }

        private static bool Same(RoleBinding fromDb, RoleBinding roleBinding, bool requireRoleId)
        {
            return fromDb.Emoji == roleBinding.Emoji && fromDb.ChannelId == roleBinding.ChannelId
                                                     && fromDb.MessageId == roleBinding.MessageId
                                                     && (fromDb.RoleId == roleBinding.RoleId || !requireRoleId);
        }
    }
}
