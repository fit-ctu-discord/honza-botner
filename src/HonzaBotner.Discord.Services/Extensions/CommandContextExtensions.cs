using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Services.Extensions;

public static class CommandContextExtensions
{
    public static async Task RespondErrorAsync(this CommandContext context, string title, string? content = null)
    {
        var embed = new DiscordEmbedBuilder()
            .WithAuthor(context.Member.DisplayName, null, context.Member.AvatarUrl)
            .WithTitle(title.RemoveDiscordMentions(context.Guild))
            .WithDescription(content?.RemoveDiscordMentions(context.Guild) ?? "No additional content provided")
            .WithColor(DiscordColor.Red)
            .WithTimestamp(DateTime.UtcNow)
            .Build();

        await context.RespondAsync(embed);
    }
}
