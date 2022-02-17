using System;
using System.Collections.Generic;
using System.Linq;
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
        DiscordRole addedRole = args.RolesAfter.First(role => !args.RolesBefore.Contains(role));

        // Has the new role some badge and has user some of the trigger roles?
        if (_options.PairedRoles.ContainsKey(addedRole.Id.ToString())
            && args.RolesBefore.Select(role => role.Id).Any(roleId => _options.TriggerRoles.Contains(roleId)))
        {
            try
            {
                DiscordRole grantedRole = args.Guild.GetRole(_options.PairedRoles[addedRole.Id.ToString()]);
                await args.Member.GrantRoleAsync(grantedRole);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed while assigning single role to {MemberName}",
                    args.Member.DisplayName);
            }
        }

        // Is the new role trigger role, and is it user's first? -> Assign all badge roles
        else if (_options.TriggerRoles.Contains(addedRole.Id) &&
                 !args.RolesBefore.Any(role => _options.TriggerRoles.Contains(role.Id)))
        {
            IEnumerable<string> pendingRoleIds =
                _options.PairedRoles.Keys.Where(id => args.RolesAfter.Select(role => role.Id.ToString()).Contains(id));
            foreach (string roleId in pendingRoleIds)
            {
                try
                {
                    DiscordRole grantedRole = args.Guild.GetRole(_options.PairedRoles[roleId]);
                    await args.Member.GrantRoleAsync(grantedRole);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Failed while assigning role {RoleId} to {MemberName}",
                        roleId, args.Member.DisplayName);
                    if (e is UnauthorizedException) continue;
                    return;
                }
            }
        }
    }

    private async Task CheckRemovedRoles(GuildMemberUpdateEventArgs args)
    {
        DiscordRole removedRole = args.RolesBefore.First(role => !args.RolesAfter.Contains(role));
        if (_options.PairedRoles.ContainsKey(removedRole.Id.ToString()))
        {
            try
            {
                DiscordRole revokedRole = args.Guild.GetRole(_options.PairedRoles[removedRole.Id.ToString()]);
                await args.Member.RevokeRoleAsync(revokedRole);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed while revoking single role from {MemberName}",
                    args.Member.DisplayName);
            }
        }
        else if (_options.TriggerRoles.Contains(removedRole.Id) &&
                 !args.RolesAfter.Any(role => _options.TriggerRoles.Contains(role.Id)))
        {
            IEnumerable<string> pendingRoleIds =
                _options.PairedRoles.Keys.Where(id => args.RolesBefore.Select(role => role.Id.ToString()).Contains(id));
            foreach (string roleId in pendingRoleIds)
            {
                try
                {
                    DiscordRole revokedRole = args.Guild.GetRole(_options.PairedRoles[roleId]);
                    await args.Member.RevokeRoleAsync(revokedRole);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Failed while revoking role {RoleId} to {MemberName}",
                        roleId, args.Member.DisplayName);
                    if (e is UnauthorizedException) continue;
                    return;
                }
            }
        }
    }
}
