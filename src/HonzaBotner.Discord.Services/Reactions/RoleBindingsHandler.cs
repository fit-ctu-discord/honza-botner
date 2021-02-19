using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Services.Contract;

namespace HonzaBotner.Discord.Services.Reactions
{
    public class RoleBindingsHandler : IReactionHandler
    {
        private readonly IRoleBindingsService _roleBindingsService;

        public RoleBindingsHandler(IRoleBindingsService roleBindingsService)
        {
            _roleBindingsService = roleBindingsService;
        }

        public async Task<IReactionHandler.Result> HandleAddAsync(MessageReactionAddEventArgs eventArgs)
        {
            ICollection<ulong> mappings =
                await _roleBindingsService.FindMappingAsync(eventArgs.Channel.Id, eventArgs.Message.Id,
                    eventArgs.Emoji.Name);
            if (!mappings.Any())
                return IReactionHandler.Result.Continue;

            DiscordMember member = await eventArgs.Guild.GetMemberAsync(eventArgs.User.Id);

            await Task.Run(async () =>
            {
                foreach (ulong roleId in mappings)
                {
                    DiscordRole? role = eventArgs.Guild.GetRole(roleId);
                    if (role == null)
                        continue;

                    await member.GrantRoleAsync(role, "Add role from binding");
                }
            });

            return IReactionHandler.Result.Stop;
        }

        public async Task<IReactionHandler.Result> HandleRemoveAsync(MessageReactionRemoveEventArgs eventArgs)
        {
            ICollection<ulong> mappings =
                await _roleBindingsService.FindMappingAsync(eventArgs.Channel.Id, eventArgs.Message.Id,
                    eventArgs.Emoji.Name);
            if (!mappings.Any())
                return IReactionHandler.Result.Continue;

            DiscordMember member = await eventArgs.Guild.GetMemberAsync(eventArgs.User.Id);

            await Task.Run(async () =>
            {
                foreach (ulong roleId in mappings)
                {
                    DiscordRole? role = eventArgs.Guild.GetRole(roleId);
                    if (role == null)
                        continue;

                    await member.RevokeRoleAsync(role, "Remove role because of binding");
                }
            });

            return IReactionHandler.Result.Stop;
        }
    }
}
