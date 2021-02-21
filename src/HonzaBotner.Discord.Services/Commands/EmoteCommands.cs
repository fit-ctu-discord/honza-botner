using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public Task PerDayCommand(CommandContext ctx, params string[] parameters)
        {
            return Display(ctx, false, parameters, emoji => emoji.UsagePerDay);
        }

        [Command("total")]
        [Aliases("all")]
        [Description("Displays total usage of emotes.")]
        public Task TotalCommand(CommandContext ctx, params string[] parameters)
        {
            return Display(ctx, true, parameters, emoji => emoji.Used);
        }

        private async Task Display<TKey>(CommandContext ctx, bool total, string[] parameters,
            Func<CountedEmoji, TKey> comparer)
        {
            GetFlags(parameters, out bool animated, out bool nonAnimated, out bool debug);
            bool all = animated == nonAnimated;

            IEnumerable<CountedEmoji> results = await _emojiCounterService.ListAsync();
            IOrderedEnumerable<CountedEmoji> orderedResults = results.OrderByDescending(comparer);

            Stopwatch stopwatch = new();
            stopwatch.Start();

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

                    if (!(emoji.IsAnimated == animated || all))
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
                        .Append("`")
                        .Append(emojisAppended % 3 == 2 ? "\n" : "\t");

                    emojisAppended++;

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

            stopwatch.Stop();
            if (debug)
            {
                await ctx.RespondAsync($"Execution of emoji print took: {stopwatch.Elapsed}");
            }
        }

        private void GetFlags(string[] parameters, out bool animated, out bool nonAnimated, out bool debug)
        {
            debug = animated = nonAnimated = false;
            foreach (string parameter in parameters)
            {
                switch (parameter)
                {
                    case "debug":
                        debug = true;
                        break;
                    case "animated":
                        animated = true;
                        break;
                    case "nonanimated":
                        nonAnimated = false;
                        break;
                    default:
                        _logger.LogInformation("Unknown flag was used in emoji print: {0}", parameter);
                        break;
                }
            }
        }
    }
}
