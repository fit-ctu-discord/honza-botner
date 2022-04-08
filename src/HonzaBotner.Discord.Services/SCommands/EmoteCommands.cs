using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using HonzaBotner.Discord.Services.Extensions;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Discord.Services.SCommands;

public class EmoteCommands : ApplicationCommandModule
{

    private readonly IEmojiCounterService _emojiCounterService;

    public EmoteCommands(IEmojiCounterService emojiCounterService)
    {
        _emojiCounterService = emojiCounterService;
    }

    [SlashCommand("emotes", "Display statistics about emotes on server", false)]
    public async Task EmoteStatsCommandAsync(
        InteractionContext ctx,
        [Choice("perDay", 0)]
        [Choice("total", 1)]
        [Option("display", "Show displayed per day or total? Defaults perDay")] long showTotal = 0,
        [Choice("all", 0)]
        [Choice("animated", 1)]
        [Choice("still", 2)]
        [Option("Type", "What type of emojis to show? Defaults all")] long type = 0)
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

            if (emoji.IsAnimated && type == 2)
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

        await ctx.DeleteResponseAsync();

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
            await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, pages);
        }
    }
}
