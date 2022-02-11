using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Attributes;

/// <summary>
/// Determines strategy of allowlist to use.
/// </summary>
public enum AllowlistStrategy
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
    private readonly AllowlistStrategy _strategy;
    private IGuildProvider? _guildProvider;
    private IList<ulong>? _roleIds;
    private ILogger<RequireAllowlistAttribute>? _logger;

    public RequireAllowlistAttribute(
        AllowlistsTypes allowlist,
        AllowlistStrategy strategy = AllowlistStrategy.Any
    )
    {
        _allowlist = allowlist;
        _strategy = strategy;
    }

    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        CommonCommandOptions? options = ctx.CommandsNext.Services.GetService<IOptions<CommonCommandOptions>>()?.Value;
        CommandAllowlistsOptions? allowlistOptions =
            ctx.CommandsNext.Services.GetService<IOptions<CommandAllowlistsOptions>>()?.Value;
        IGuildProvider? guildProvider = ctx.Services.GetService<IGuildProvider>();
        if (options is null || allowlistOptions is null || guildProvider is null) return false;

        _guildProvider = guildProvider;
        _logger = ctx.CommandsNext.Services.GetService<ILogger<RequireAllowlistAttribute>>();

        DiscordGuild guild = await guildProvider.GetCurrentGuildAsync();
        DiscordMember member = await guild.GetMemberAsync(ctx.User.Id);

        _roleIds = allowlistOptions.GetAllowlistFromItsType(_allowlist);
        return member.Roles.Contains(guild.GetRole(options.ModRoleId)) || _strategy switch
        {
            AllowlistStrategy.All => _roleIds.All(
                role => member.Roles.Select(r => r.Id).Contains(role)
            ),
            AllowlistStrategy.Any => _roleIds.Any(
                role => member.Roles.Select(r => r.Id).Contains(role)
            ),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override async Task<DiscordEmbed> BuildFailedCheckDiscordEmbed()
    {
        string logicString = _strategy switch
        {
            AllowlistStrategy.All => "s právě všemi rolemi",
            AllowlistStrategy.Any => "s alespoň jednou rolí",
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
            catch (Exception e)
            {
                _logger?.LogWarning(
                    e,
                    "Unknown role {RoleId} used in allowlist {Allowlist}",
                    roleId, _allowlist
                );
            }
        }

        return string.Join(", ", roles);
    }
}
