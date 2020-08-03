using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Command.Permissions.Data;

namespace HonzaBotner.Discord.Command.Permissions
{
    internal class PermissionResolver : IPermissionResolver
    {
        private readonly IPermissionEntriesRepository _repository;

        public PermissionResolver(IPermissionEntriesRepository repository)
        {
            _repository = repository;
        }

        public Task<bool> IsUserEligibleToRunAsync(DiscordMember user, IChatCommand command)
        {
            return IsUserEligibleToRunAsync(user, command.RequiredPermissions);
        }

        public async Task<bool> IsUserEligibleToRunAsync(DiscordMember user, IEnumerable<string> requiredPermissions)
        {
            // Administrators are always allowed to run any command inside their guild
            if (IsAdministrator(user))
            {
                return true;
            }

            IEnumerable<string> permissions = requiredPermissions.ToList();
            var relevantEntries = await _repository.GetPermissionEntriesByPermissionsAsync(permissions);
            var denials = relevantEntries.Where(entry => entry.Type == PermissionEntryType.Denial);

            // If there is a explicit denial for the user of any of his roles, it has higher precedence than grants.
            if (denials.Any(entry => EntryMatchesUser(entry, user)))
            {
                return false;
            }

            var grants = relevantEntries.Where(entry => entry.Type == PermissionEntryType.Grant);

            // Check, whether the user has all required permissions. Permissions can be granted either to an user explicitly
            // or to a role, that the given user has
            return permissions.All(permission =>
                grants.Any(entry => entry.Permission == permission && EntryMatchesUser(entry, user)));
        }

        private static bool EntryMatchesUser(PermissionEntry entry, DiscordMember user)
        {
            return entry.Target switch
            {
                PermissionEntryTarget.User => user.Id == entry.TargetId,
                PermissionEntryTarget.Role => user.Roles.Any(role => role.Id == entry.TargetId),
                PermissionEntryTarget.Everyone => true,
                // How does this even happen?
                _ => throw new ArgumentOutOfRangeException(nameof(entry.Target))
            };
        }

        private static bool IsAdministrator(DiscordMember user)
        {
            // All users with MANAGE_SERVER (in terms of Discord API glossary) are considered administrators
            return user.PermissionsIn(user.Guild.GetDefaultChannel()) == DSharpPlus.Permissions.ManageGuild;
        }
    }
}
