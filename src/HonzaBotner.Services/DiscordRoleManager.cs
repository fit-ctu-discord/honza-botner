using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IGuildProvider _guildProvider;
        private readonly DiscordRoleConfig _roleConfig;

        public DiscordRoleManager(IOptions<DiscordRoleConfig> options, IGuildProvider guildProvider)
        {
            _guildProvider = guildProvider;
            _roleConfig = options.Value;
        }

        public async Task<bool> GrantRolesAsync(ulong userId, IReadOnlySet<DiscordRole> discordRoles)
        {
            DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();

            List<DRole> roles = new();
            foreach (DiscordRole discordRole in discordRoles)
            {
                DRole? role = guild.GetRole(discordRole.RoleId);
                if (role == null)
                {
                    return false;
                }

                roles.Add(role);
            }

            DiscordMember member = await guild.GetMemberAsync(userId);
            foreach (DRole role in roles)
            {
                await member.GrantRoleAsync(role, "Auth");
            }

            return true;
        }

        public async Task<bool> UngrantRolesPoolAsync(ulong userId, RolesPool rolesPool)
        {
            DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();

            IDictionary<string, ulong> rolesMapping = rolesPool switch
            {
                RolesPool.Auth => _roleConfig.RoleMapping,
                RolesPool.Staff => _roleConfig.StaffRoleMapping,
                _ => throw new ArgumentOutOfRangeException(nameof(rolesPool), rolesPool, null)
            };

            List<DRole> roles = new();

            foreach ((var key, ulong value) in rolesMapping)
            {
                DRole? role = guild.GetRole(value);
                if (role == null)
                {
                    return false;
                }

                roles.Add(role);
            }

            DiscordMember member = await guild.GetMemberAsync(userId);
            foreach (DRole role in roles)
            {
                await member.RevokeRoleAsync(role, "Auth");
            }

            return true;
        }

        public HashSet<DiscordRole> MapUsermapRoles(IReadOnlyCollection<string> kosRoles, RolesPool rolesPool)
        {
            HashSet<DiscordRole> discordRoles = new();


            IDictionary<string, ulong> roles = rolesPool switch
            {
                RolesPool.Auth => _roleConfig.RoleMapping,
                RolesPool.Staff => _roleConfig.StaffRoleMapping,
                _ => throw new ArgumentOutOfRangeException(nameof(rolesPool), rolesPool, null)
            };

            IEnumerable<string> knowUserRolePrefixes = roles.Keys;

            foreach (string rolePrefix in knowUserRolePrefixes)
            {
                bool containsRole = kosRoles.Any(role => role.StartsWith(rolePrefix));

                if (containsRole)
                {
                    discordRoles.Add(new DiscordRole(roles[rolePrefix]));
                }
            }

            return discordRoles;
        }
    }
}
