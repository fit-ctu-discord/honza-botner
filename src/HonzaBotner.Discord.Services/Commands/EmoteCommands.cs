using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Discord.Services.Commands;

[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public class EmoteCommands : ApplicationCommandModule
{

    private readonly IEmojiCounterService _emojiCounterService;

    public enum DisplayTypes
    {
        [ChoiceName("emotes")]
        All,
        [ChoiceName("animated")]
        Animated,
        [ChoiceName("still")]
        Still,
        [ChoiceName("stickers")]
        Stickers
    }

    public EmoteCommands(IEmojiCounterService emojiCounterService)
    {
        _emojiCounterService = emojiCounterService;
    }

    [SlashCommand("emotes", "Display statistics about emotes on server")]
    [SlashCommandPermissions(Permissions.ManageEmojis)]
    public async Task EmoteStatsCommandAsync(
        InteractionContext ctx,
        [Option("display", "Display as total instead of perDay?")] bool total = false,
        [Option("type", "What type of emojis to show? Defaults all")] DisplayTypes type = DisplayTypes.All)
    {
        IEnumerable<CountedEmoji> results = await _emojiCounterService.ListAsync();
        IOrderedEnumerable<CountedEmoji> orderedResults = total
            ? results.OrderByDescending(emoji => emoji.Used)
            : results.OrderByDescending(emoji => emoji.UsagePerDay);

        if (type == DisplayTypes.Stickers) await DisplayStickersAsync(ctx, total, orderedResults);
        else await DisplayEmotesAsync(ctx, total, type, orderedResults);
    }

    private async Task DisplayEmotesAsync(
        InteractionContext ctx,
        bool total,
        DisplayTypes type,
        IOrderedEnumerable<CountedEmoji> orderedResults)
    {
        StringBuilder builder = new("\n");

        int emojisAppended = 0;

        IReadOnlyDictionary<ulong, DiscordEmoji> emojis = ctx.Guild.Emojis;

        foreach (CountedEmoji result in orderedResults)
        {
            if (!emojis.TryGetValue(result.Id, out DiscordEmoji? emoji))
            {
                continue;
            }

            if (emoji.IsAnimated && type == DisplayTypes.Still)
            {
                continue;
            }

            if (!emoji.IsAnimated && type == DisplayTypes.Animated)
            {
                continue;
            }

            string label = total ? "×" : "×/day";

            builder.Append(emoji)
                .Append("`")
                .Append(
                    (total
                        ? result.Used.ToString()
                        : $"{result.UsagePerDay:0.00}").PadLeft(10, ' '))
                .Append(label)
                .Append(" `")
                .Append(emojisAppended % 3 == 2 ? "\n" : "\t");

            emojisAppended++;
        }

        if (emojisAppended > 0)
        {
            InteractivityExtension? interactivity = ctx.Client.GetInteractivity();
            DiscordEmbedBuilder embedBuilder = new()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = ctx.Member.AvatarUrl, Name = ctx.Member.DisplayName
                },
                Title = "Custom emotes usage stats"
            };
            IEnumerable<Page> pages = interactivity.GeneratePagesInEmbed(builder.ToString(), SplitType.Line, embedBuilder);
            await interactivity.SendPaginatedResponseAsync(ctx.Interaction, false, ctx.User, pages);
        }
        else
        {
            await ctx.CreateResponseAsync("No emotes to show", true);
        }
    }

    private async Task DisplayStickersAsync(
        InteractionContext ctx,
        bool total,
        IOrderedEnumerable<CountedEmoji> orderedResults)
    {
        StringBuilder builder = new("\n");

        int stickersAppended = 0;

        IReadOnlyDictionary<ulong, DiscordMessageSticker> stickers = ctx.Guild.Stickers;

        foreach (CountedEmoji result in orderedResults)
        {
            if (!stickers.Keys.Contains(result.Id))
            {
                continue;
            }
            string label = total ? "×" : "×/day";

            builder.Append(stickers[result.Id].Name)
                .Append(" `")
                .Append(
                    (total ? result.Used.ToString() : $"{result.UsagePerDay:0.00}").PadLeft(10, ' '))
                .Append(label)
                .Append(" `\n");

            stickersAppended++;
        }

        if (stickersAppended > 0)
        {
            InteractivityExtension? interactivity = ctx.Client.GetInteractivity();
            DiscordEmbedBuilder embedBuilder = new()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = ctx.Member.AvatarUrl, Name = ctx.Member.DisplayName
                },
                Title = "Custom sticker usage stats"
            };
            IEnumerable<Page> pages = interactivity.GeneratePagesInEmbed(builder.ToString(), SplitType.Line, embedBuilder);
            await interactivity.SendPaginatedResponseAsync(ctx.Interaction, false, ctx.User, pages);
        }
        else
        {
            await ctx.CreateResponseAsync("No stickers to show", true);
        }
    }
}
