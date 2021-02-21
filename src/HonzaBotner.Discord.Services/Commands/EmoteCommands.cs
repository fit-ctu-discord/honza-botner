using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("emotes")]
    [Aliases("emote", "emojis", "emoji")]
    [Description(
        "Commands to display stats about emote usage. You can also use additional switches `animated` and `nonanimated`.")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [Cooldown(2, 120, CooldownBucketType.User)]
    public class EmoteCommands : BaseCommandModule
    {
        private readonly IEmojiCounterService _emojiCounterService;
        private readonly ILogger<EmoteCommands> _logger;

        public EmoteCommands(IEmojiCounterService emojiCounterService, ILogger<EmoteCommands> logger)
        {
            _emojiCounterService = emojiCounterService;
            _logger = logger;
        }

        [GroupCommand]
        [Command("perday")]
        [Aliases("daily")]
        [Description("Displays per day usage of emotes.")]
        public Task PerDayCommand(CommandContext ctx, [RemainingText] string parameters)
        {
            return Display(ctx, false, parameters, emoji => emoji.UsagePerDay);
        }

        [Command("total")]
        [Aliases("all")]
        [Description("Displays total usage of emotes.")]
        public Task TotalCommand(CommandContext ctx)
        {
            return Display(ctx, true, "", emoji => emoji.Used);
        }

        private async Task Display<TKey>(CommandContext ctx, bool total, string parameters,
            Func<CountedEmoji, TKey> comparer)
        {
            IEnumerable<CountedEmoji> results = await _emojiCounterService.ListAsync();
            IOrderedEnumerable<CountedEmoji> orderedResults = results.OrderByDescending(comparer);

            await ctx.RespondAsync("**Statistika používání custom emotes**");

            StringBuilder builder = new();
            builder.Append("\n");

            int emojisAppended = 0;
            const int chunkSize = 30;

            IReadOnlyDictionary<ulong, DiscordEmoji> emojis = ctx.Guild.Emojis;

            foreach (CountedEmoji result in orderedResults)
            {
                try
                {
                    if (!emojis.TryGetValue(result.Id, out DiscordEmoji? emoji) || emoji == null)
                    {
                        continue;
                    }

                    {
                        string label = total ? "×" : "×/day";

                        builder.Append(emoji)
                            .Append("`")
                            .Append(
                                (total
                                    ? result.Used.ToString()
                                    : $"{result.UsagePerDay:0.00}").PadLeft(10, ' '))
                            .Append(label)
                            .Append("`")
                            .Append(emojisAppended % 3 == 2 ? "\n" : "\t");

                        emojisAppended++;
                    }

                    if (emojisAppended == chunkSize)
                    {
                        await ctx.RespondAsync(builder.ToString());
                        builder.Clear();
                        builder.Append("\n");
                        emojisAppended = 0;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Couldn't find emote with id {0}", result.Id);
                }
            }

            string remaining = builder.ToString();
            if (!string.IsNullOrEmpty(remaining.Trim()))
            {
                await ctx.RespondAsync(builder.ToString());
            }

            builder.Clear();
        }
    }
}
