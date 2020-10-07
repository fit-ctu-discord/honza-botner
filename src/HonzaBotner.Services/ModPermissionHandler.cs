using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord;

namespace HonzaBotner.Services
{
    public class ModPermissionHandler : IPermissionHandler
    {
        private readonly IGuildProvider _guildProvider;
        private readonly IPermissionHandler _permissionHandler;

        public ModPermissionHandler(IGuildProvider guildProvider, IPermissionHandler permissionHandler)
        {
            _guildProvider = guildProvider;
            _permissionHandler = permissionHandler;
        }

        public async Task<bool> HasRightsAsync(ulong userId, CommandPermission requiredPermission)
        {
            if (requiredPermission == CommandPermission.Mod)
            {
                IEnumerable<ulong> elevatedRolesIds = _guildProvider.GetElevatedRoleIds();
                DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();
                DiscordMember member = await guild.GetMemberAsync(userId);

                if (member.Roles.Any(r =>
                    elevatedRolesIds.Any(id => r.Id == id)))
                {
                    return true;
                }
            }

            return await _permissionHandler.HasRightsAsync(userId, requiredPermission);
        }
    }
}
