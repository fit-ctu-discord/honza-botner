using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using HonzaBotner.Discord.Services.Extensions;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Discord.Services.Commands;

public class EmoteCommands : ApplicationCommandModule
{

    private readonly IEmojiCounterService _emojiCounterService;

    public enum DisplayTypes
    {
        [ChoiceName("all")]
        All,
        [ChoiceName("animated")]
        Animated,
        [ChoiceName("still")]
        Still
    }

    public EmoteCommands(IEmojiCounterService emojiCounterService)
    {
        _emojiCounterService = emojiCounterService;
    }

    [SlashCommand("emotes", "Display statistics about emotes on server")]
    [SlashCommandPermissions(Permissions.ManageEmojis)]
    public async Task EmoteStatsCommandAsync(
        InteractionContext ctx,
        [Choice("perDay", 0)]
        [Choice("total", 1)]
        [Option("display", "Show displayed per day or total? Defaults perDay")] long showTotal = 0,
        [Option("Type", "What type of emojis to show? Defaults all")] DisplayTypes type = DisplayTypes.All)
    {
        bool total = showTotal == 1;
        await ctx.DeferAsync();

        IEnumerable<CountedEmoji> results = await _emojiCounterService.ListAsync();
        IOrderedEnumerable<CountedEmoji> orderedResults = total
            ? results.OrderByDescending(emoji => emoji.Used)
            : results.OrderByDescending(emoji => emoji.UsagePerDay);

        StringBuilder builder = new("\n");

        int emojisAppended = 0;
        //const int chunkSize = 30;

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
                Title = "Statistika používání custom emotes"
            };
            IEnumerable<Page> pages = interactivity.GeneratePages(builder.ToString(), embedBuilder, 12);
            await ctx.CreateResponseAsync("Done", true);
            await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, pages);
        }
        else
        {
            await ctx.CreateResponseAsync("No emojis to show", true);
        }
    }
}
