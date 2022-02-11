using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Attributes;

/// <summary>
///
/// </summary>
public enum RoleLogic
{
    All,
    Any
}

/// <summary>
/// Defines that usage of this command is only allowed to:
/// - moderators
/// - or members that are allow-listed (in config) to do so.
/// </summary>
public class RequireAllowlistAttribute : CustomBaseAttribute
{
    private readonly AllowlistsTypes _allowlist;
    private readonly RoleLogic _roleLogic;
    private IGuildProvider? _guildProvider;
    private IList<ulong>? _roleIds;

    public RequireAllowlistAttribute(
        AllowlistsTypes allowlist,
        RoleLogic roleLogic = RoleLogic.Any
    )
    {
        _allowlist = allowlist;
        _roleLogic = roleLogic;
    }

    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        CommonCommandOptions? options = ctx.CommandsNext.Services.GetService<IOptions<CommonCommandOptions>>()?.Value;
        CommandAllowlistsOptions? allowlistOptions =
            ctx.CommandsNext.Services.GetService<IOptions<CommandAllowlistsOptions>>()?.Value;
        IGuildProvider? guildProvider = ctx.Services.GetService<IGuildProvider>();
        if (options is null || allowlistOptions is null || guildProvider is null) return false;
        _guildProvider = guildProvider;
        DiscordGuild guild = await guildProvider.GetCurrentGuildAsync();
        DiscordMember member = await guild.GetMemberAsync(ctx.User.Id);

        _roleIds = allowlistOptions.GetAllowlistFromItsType(_allowlist);
        return member.Roles.Contains(guild.GetRole(options.ModRoleId)) || _roleLogic switch
        {
            RoleLogic.All => _roleIds.All(
                role => member.Roles.Select(r => r.Id).Contains(role)
            ),
            RoleLogic.Any => _roleIds.Any(
                role => member.Roles.Select(r => r.Id).Contains(role)
            ),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override async Task<DiscordEmbed> BuildFailedCheckDiscordEmbed()
    {
        string logicString = _roleLogic switch
        {
            RoleLogic.All => "s právě všemi rolemi",
            RoleLogic.Any => "s alespoň jednou rolí",
            _ => throw new ArgumentOutOfRangeException()
        };

        return new DiscordEmbedBuilder()
            .WithTitle("Přístup zakázán")
            .WithDescription(
                $"Tento příkaz může používat pouze Moderátor nebo člen {logicString}: {await ConstructAllAllowlistedRoles()}.")
            .WithColor(DiscordColor.Violet)
            .Build();
    }

    private async Task<string> ConstructAllAllowlistedRoles()
    {
        if (_guildProvider == null || _roleIds == null) return "?";

        List<string> roles = new();
        DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();
        foreach (ulong roleId in _roleIds)
        {
            try
            {
                DiscordRole role = guild.GetRole(roleId);
                roles.Add(role.Name);
            }
            catch (Exception)
            {
                // TODO: how to add logger here?
            }
        }

        return string.Join(", ", roles);
    }
}
