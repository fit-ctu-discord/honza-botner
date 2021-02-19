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
                await _roleBindingsService.FindMappingAsync(eventArgs.Channel.Id, eventArgs.Message.Id, eventArgs.Emoji.Name);
            if (!mappings.Any())
                return IReactionHandler.Result.Continue;

            foreach (ulong roleId in mappings)
            {
                DiscordRole? role = GetRole(eventArgs.Guild, roleId);
                if(role == null)
                    continue;

                DiscordMember member = await GetMemberAsync(eventArgs.Guild, eventArgs.User.Id);

                await member.GrantRoleAsync(role, "Zira");
            }

            return IReactionHandler.Result.Stop;
        }

        public async Task<IReactionHandler.Result> HandleRemoveAsync(MessageReactionRemoveEventArgs eventArgs)
        {
            ICollection<ulong> mappings =
                await _roleBindingsService.FindMappingAsync(eventArgs.Channel.Id, eventArgs.Message.Id, eventArgs.Emoji.Name);
            if (!mappings.Any())
                return IReactionHandler.Result.Continue;

            foreach (ulong roleId in mappings)
            {
                DiscordRole? role = GetRole(eventArgs.Guild, roleId);
                if(role == null)
                    continue;

                DiscordMember member = await GetMemberAsync(eventArgs.Guild, eventArgs.User.Id);

                await member.RevokeRoleAsync(role, "Zira");
            }

            return IReactionHandler.Result.Stop;
        }

        private Task<DiscordMember> GetMemberAsync(DiscordGuild guild, ulong userId)
        {
            return guild.GetMemberAsync(userId);
        }

        private DiscordRole GetRole(DiscordGuild guild, ulong roleId)
        {
            return guild.GetRole(roleId);
        }
    }
}
