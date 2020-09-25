using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Options;
using DiscordRole = HonzaBotner.Services.Contract.Dto.DiscordRole;
using DRole = DSharpPlus.Entities.DiscordRole;

namespace HonzaBotner.Services
{
    public sealed class DiscordRoleManager : IDiscordRoleManager
    {
        private readonly DiscordRoleConfig _roleConfig;
        private readonly DiscordClient _client;

        public DiscordRoleManager(IOptions<DiscordRoleConfig> options, DiscordWrapper wrapper)
        {
            _roleConfig = options.Value;
            _client = wrapper.Client;
        }

        public async Task<bool> GrantRolesAsync(ulong guildId, ulong userId, IEnumerable<DiscordRole> discordRoles)
        {
            // TODO: Do this in service.
            DiscordGuild? guild = await _client.GetGuildAsync(guildId);
            if (guild == null)
            {
                return false;
            }

            DiscordMember? member = await guild.GetMemberAsync(userId);
            if (member == null)
            {
                return false;
            }

            List<DRole> roles = new List<DRole>();
            foreach (DiscordRole discordRole in discordRoles)
            {
                DRole? role = guild.GetRole(discordRole.RoleId);
                if (role == null)
                {
                    return false;
                }

                roles.Add(role);
            }

            foreach (DRole role in roles)
            {
                await member.GrantRoleAsync(role, "Auth");
            }

            return true;
        }

        public HashSet<DiscordRole> MapUsermapRoles(params string[] kosRoles)
        {
            HashSet<DiscordRole> discordRoles = new HashSet<DiscordRole>();

            IEnumerable<string> knowUserRolePrefixes = _roleConfig.RoleMapping.Keys;

            foreach (string roleName in kosRoles)
            {
                string? rolePrefix = knowUserRolePrefixes.FirstOrDefault(prefix => roleName.StartsWith(prefix));

                if (rolePrefix != null)
                {
                    discordRoles.Add(new DiscordRole(_roleConfig.RoleMapping[rolePrefix]));
                }
            }

            return discordRoles;
        }
    }
}
