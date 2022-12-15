using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers;

public class BadgeRoleHandler : IEventHandler<GuildMemberUpdateEventArgs>
{
    private readonly ILogger<BadgeRoleHandler> _logger;
    private readonly BadgeRoleOptions _options;

    public BadgeRoleHandler(IOptions<BadgeRoleOptions> options, ILogger<BadgeRoleHandler> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<EventHandlerResult> Handle(GuildMemberUpdateEventArgs args)
    {
        if (args.RolesAfter.Count > args.RolesBefore.Count) _ = Task.Run(() => CheckAddedRoles(args));
        else if (args.RolesAfter.Count < args.RolesBefore.Count) _ = Task.Run(() => CheckRemovedRoles(args));
        return Task.FromResult(EventHandlerResult.Continue);
    }

    private async Task CheckAddedRoles(GuildMemberUpdateEventArgs args)
    {
        // Is any of the current roles trigger role
        // and does the member have a badge role
        // and does not the member already have a badge
        if (_options.TriggerRoles.Any(role => args.RolesAfter.Select(roleA => roleA.Id).Contains(role))
            && _options.PairedRoles.Keys.Any(role => args.RolesAfter.Select(roleA => roleA.Id.ToString()).Contains(role))
            && !_options.PairedRoles.Values.Any(role => args.RolesAfter.Select(roleA => roleA.Id).Contains(role)))
        {
            try
            {
                DiscordRole grantedRole = args.Guild.GetRole(
                    _options.PairedRoles.First(
                        roleId => args.RolesAfter.Select(role => role.Id.ToString()).Contains(roleId.Key)
                        ).Value
                    );
                await args.Member.GrantRoleAsync(grantedRole);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed while assigning single role to {MemberName}",
                    args.Member.DisplayName);
            }
        }
    }

    private async Task CheckRemovedRoles(GuildMemberUpdateEventArgs args)
    {
        // Is NOT any of the current roles trigger role
        // or does the member have NOT a badge role
        // and does not the member have a badge
        if ((
                !_options.TriggerRoles.Any(role => args.RolesAfter.Select(roleA => roleA.Id).Contains(role))
                || !_options.PairedRoles.Keys.Any(role => args.RolesAfter.Select(roleA => roleA.Id.ToString()).Contains(role))
                )
            && !_options.PairedRoles.Values.Any(role => args.RolesAfter.Select(roleA => roleA.Id).Contains(role)))
        {
            try
            {
                DiscordRole revokedRole = args.Guild.GetRole(
                    _options.PairedRoles.First(
                        roleId => args.RolesAfter.Select(role => role.Id.ToString()).Contains(roleId.Key)
                    ).Value
                );
                await args.Member.RevokeRoleAsync(revokedRole);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed while revoking single role to {MemberName}",
                    args.Member.DisplayName);
            }
        }
    }
}
