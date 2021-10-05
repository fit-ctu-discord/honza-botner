using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using HonzaBotner.Discord.Services.Extensions;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.SlashCommands
{
    [SlashCommandGroup("emoji", "prikazy k zobrazeni statistik emoji")]
    public class EmojiSCommands : ApplicationCommandModule
    {

        private readonly IEmojiCounterService _emojiCounter;
        private readonly ILogger<EmojiSCommands> _logger;

        public EmojiSCommands(ILogger<EmojiSCommands> logger, IEmojiCounterService service)
        {
            _emojiCounter = service;
            _logger = logger;
        }

        [SlashCommand("stats", "get emoji stats of this server", false)]
        [SlashRequireGuild]
        public async Task EmojiStatsAsync(
            InteractionContext ctx,
            [Option("type", "Display stats per day or total?")]
            [Choice("Per day", "perDay")]
            [Choice("Total", "total")]
            string statsType = "perDay",
            [Option("EmojiType", "Only animated or static?")]
            [Choice("animated", "animated")]
            [Choice("static", "null")]
            [Choice("all", "all")]
            string emojiType = "all")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            bool isTotal = statsType switch
            {
                "perDay" => false,
                "total" => true,
                _ => throw new ArgumentException("Invalid argument", statsType)
            };

            IEnumerable<CountedEmoji> results = await _emojiCounter.ListAsync();
            IOrderedEnumerable<CountedEmoji> orderedResults;

            if (isTotal)
            {
                orderedResults = results.OrderByDescending(emoji => emoji.Used);
            }
            else
            {
                orderedResults = results.OrderByDescending(emoji => emoji.UsagePerDay);

            }
            StringBuilder builder = new();
            builder.Append("\n");

            int emojisAppended = 0;
            //const int chunkSize = 30;

            IReadOnlyDictionary<ulong, DiscordEmoji> emojis = ctx.Guild.Emojis;

            foreach (CountedEmoji result in orderedResults)
            {
                if (!emojis.TryGetValue(result.Id, out DiscordEmoji? emoji))
                {
                    continue;
                }

                if (!((emoji.IsAnimated && emojiType == "animated") || emojiType == "all"))
                {
                    continue;
                }

                string label = isTotal ? "×" : "×/day";

                builder.Append(emoji)
                    .Append("`")
                    .Append(
                        (isTotal
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
                await ctx.CreateResponseAsync(InteractionResponseType.Pong);
                IEnumerable<Page> pages = interactivity.GeneratePages(builder.ToString(), embedBuilder, 12);
                await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, pages);
            }

        }

    }
}
