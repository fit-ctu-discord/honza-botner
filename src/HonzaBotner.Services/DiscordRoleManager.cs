using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using HonzaBotner.Discord;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DiscordRole = HonzaBotner.Services.Contract.Dto.DiscordRole;
using DRole = DSharpPlus.Entities.DiscordRole;

namespace HonzaBotner.Services
{
    public sealed class DiscordRoleManager : IDiscordRoleManager
    {
        private readonly ILogger<DiscordRoleManager> _logger;
        private readonly IGuildProvider _guildProvider;
        private readonly DiscordRoleConfig _roleConfig;

        public DiscordRoleManager(IOptions<DiscordRoleConfig> options, ILogger<DiscordRoleManager> logger,
            IGuildProvider guildProvider)
        {
            _logger = logger;
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
                    _logger.LogWarning("Couldn't map all roles. Please verify your config file");
                    return false;
                }

                roles.Add(role);
            }

            // TODO: job queue
            Task _ = Task.Run(async () =>
            {
                DiscordMember member = await guild.GetMemberAsync(userId);
                foreach (DRole role in roles)
                {
                    await member.GrantRoleAsync(role, "Auth");
                }
            });

            return true;
        }

        public async Task<bool> RevokeRolesPoolAsync(ulong userId, RolesPool rolesPool)
        {
            bool returnValue = true;
            DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();

            IDictionary<string, ulong[]> rolesMapping = rolesPool switch
            {
                RolesPool.Auth => _roleConfig.AuthRoleMapping,
                RolesPool.Staff => _roleConfig.StaffRoleMapping,
                _ => throw new ArgumentOutOfRangeException(nameof(rolesPool), rolesPool, null)
            };

            List<DRole> roles = new();

            foreach ((string? key, ulong[] roleIds) in rolesMapping)
            {
                foreach (ulong value in roleIds)
                {
                    DRole? role = guild.GetRole(value);
                    if (role == null)
                    {
                        returnValue = false;
                        _logger.LogError("Revoking roles for user id {UserId} failed for role key:{Key} = value:{Value}",
                            userId, key, value);
                    }
                    else
                    {
                        roles.Add(role);
                    }
                }
            }

            // TODO: job queue
            DiscordMember member = await guild.GetMemberAsync(userId);
            foreach (DRole role in roles)
            {
                if (member.Roles.Contains(role))
                {
                    await member.RevokeRoleAsync(role, "Auth");
                }
            }

            return returnValue;
        }

        public HashSet<DiscordRole> MapUsermapRoles(IReadOnlyCollection<string> kosRoles, RolesPool rolesPool)
        {
            HashSet<DiscordRole> discordRoles = new();

            IDictionary<string, ulong[]> roles = rolesPool switch
            {
                RolesPool.Auth => _roleConfig.AuthRoleMapping,
                RolesPool.Staff => _roleConfig.StaffRoleMapping,
                _ => throw new ArgumentOutOfRangeException(nameof(rolesPool), rolesPool, null)
            };

            IEnumerable<string> knowUserRolePrefixes = roles.Keys;

            foreach (string rolePrefix in knowUserRolePrefixes)
            {
                bool containsRole = kosRoles.Any(role => role.StartsWith(rolePrefix));

                if (containsRole)
                {
                    foreach (DiscordRole role in roles[rolePrefix].Select(roleId => new DiscordRole(roleId)))
                    {
                        discordRoles.Add(role);
                    }
                }
            }

            return discordRoles;
        }

        public async Task RevokeHostRolesAsync(ulong userId)
        {
            DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();
            DiscordMember member = await guild.GetMemberAsync(userId);

            // TODO: somehow merge with Revoke part later.
            foreach (ulong roleId in _roleConfig.HostRoleIds)
            {
                try
                {
                    DRole role = guild.GetRole(roleId);
                    await member.RevokeRoleAsync(role);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Revoking host role '{RoleId}' failed", roleId);
                }
            }
        }
    }
}
