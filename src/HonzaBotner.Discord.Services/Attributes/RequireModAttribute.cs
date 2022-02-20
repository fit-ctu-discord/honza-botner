using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Attributes;

/// <summary>
/// Defines that usage of this command is only allowed to moderators.
/// </summary>
public class RequireModAttribute : CustomBaseAttribute
{
    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        CommonCommandOptions? options = ctx.CommandsNext.Services.GetService<IOptions<CommonCommandOptions>>()?.Value;
        IGuildProvider? guildProvider = ctx.Services.GetService<IGuildProvider>();
        if (options is null || guildProvider is null) return false;
        DiscordGuild guild = await guildProvider.GetCurrentGuildAsync();
        DiscordMember member = await guild.GetMemberAsync(ctx.User.Id);

        return member.Roles.Contains(guild.GetRole(options.ModRoleId));
    }

    public override Task<DiscordEmbed> BuildFailedCheckDiscordEmbed()
    {
        return Task.FromResult(
            new DiscordEmbedBuilder()
                .WithTitle("Přístup zakázán")
                .WithDescription("Tento příkaz může používat pouze Moderátor.")
                .WithColor(DiscordColor.Violet)
                .Build()
        );
    }
}
