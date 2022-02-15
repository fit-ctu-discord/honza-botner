using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers;

public class BadgeRoleHandler
{
    private readonly ILogger<BadgeRoleHandler> _logger;
    private readonly BadgeRoleOptions _options;

    public BadgeRoleHandler(IOptions<BadgeRoleOptions> options, ILogger<BadgeRoleHandler> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<EventHandlerResult> Handle(GuildMemberUpdateEventArgs args)
    {
        await Task.Delay(0); // Avoid warning that there is no "await" in async method
        if (args.RolesAfter.Count > args.RolesBefore.Count) await CheckAddedRoles(args);
        else if (args.RolesAfter.Count < args.RolesBefore.Count) await CheckRemovedRoles(args);
        return EventHandlerResult.Continue;
    }

    private async Task CheckAddedRoles(GuildMemberUpdateEventArgs args)
    {
        DiscordRole addedRole = args.RolesAfter.First(role => !args.RolesBefore.Contains(role));
        if (_options.PairedRoles.ContainsKey(addedRole.Id))
        {
            DiscordRole grantedRole = args.Guild.GetRole(_options.PairedRoles[addedRole.Id]);
            await args.Member.GrantRoleAsync(grantedRole);
        }
        else if (_options.TriggerRoles.Contains(addedRole.Id) &&
                 !args.RolesBefore.Any(role => _options.TriggerRoles.Contains(role.Id)))
        {
            IEnumerable<ulong> pendingRoleIds =
                _options.PairedRoles.Keys.Where(id => args.RolesAfter.Select(role => role.Id).Contains(id));
            foreach (ulong roleId in pendingRoleIds)
            {
                DiscordRole grantedRole = args.Guild.GetRole(_options.PairedRoles[roleId]);
                await args.Member.GrantRoleAsync(grantedRole);
            }
        }
    }

    private async Task CheckRemovedRoles(GuildMemberUpdateEventArgs args)
    {
        DiscordRole removedRole = args.RolesBefore.First(role => !args.RolesAfter.Contains(role));
        if (_options.PairedRoles.ContainsKey(removedRole.Id))
        {
            DiscordRole revokedRole = args.Guild.GetRole(_options.PairedRoles[removedRole.Id]);
            await args.Member.RevokeRoleAsync(revokedRole);
        }
        else if (_options.TriggerRoles.Contains(removedRole.Id) &&
                 args.RolesAfter.Any(role => _options.TriggerRoles.Contains(role.Id)))
        {
            IEnumerable<ulong> pendingRoleIds =
                _options.PairedRoles.Keys.Where(id => args.RolesBefore.Select(role => role.Id).Contains(id));
            foreach (ulong roleId in pendingRoleIds)
            {
                DiscordRole revokedRole = args.Guild.GetRole(_options.PairedRoles[roleId]);
                await args.Member.RevokeRoleAsync(revokedRole);
            }
        }
    }
}
