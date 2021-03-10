using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using HonzaBotner.Discord.Extensions;
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
    [Cooldown(1, 60, CooldownBucketType.Guild)]
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
            GetFlags(parameters, out bool animated, out bool nonAnimated);
            bool all = animated == nonAnimated;

            IEnumerable<CountedEmoji> results = await _emojiCounterService.ListAsync();
            IOrderedEnumerable<CountedEmoji> orderedResults = results.OrderByDescending(comparer);

            StringBuilder builder = new();
            builder.Append("\n");

            int emojisAppended = 0;
            //const int chunkSize = 30;

            IReadOnlyDictionary<ulong, DiscordEmoji> emojis = ctx.Guild.Emojis;

            foreach (CountedEmoji result in orderedResults)
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
                        IconUrl = ctx.Member.AvatarUrl, Name = ctx.Member.RatherNicknameThanUsername()
                    },
                    Title = "Statistika používání custom emotes"
                };
                IEnumerable<Page> pages = interactivity.GeneratePages(builder.ToString(), embedBuilder, 12);
                await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, pages);
            }
        }

        private void GetFlags(string[] parameters, out bool animated, out bool nonAnimated)
        {
            animated = nonAnimated = false;
            foreach (string parameter in parameters)
            {
                switch (parameter)
                {
                    case "animated":
                        animated = true;
                        break;
                    case "nonanimated":
                        nonAnimated = true;
                        break;
                    default:
                        _logger.LogInformation("Unknown flag was used in emoji print: {Parameter}", parameter);
                        break;
                }
            }
        }
    }
}
