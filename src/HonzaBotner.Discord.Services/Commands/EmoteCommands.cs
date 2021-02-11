using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("emotes")]
    [Description("Příkazy ke získání informací ohledně emotes")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class EmoteCommands : BaseCommandModule
    {
        private readonly IEmojiCounterService _emojiCounterService;

        public EmoteCommands(IEmojiCounterService emojiCounterService)
        {
            _emojiCounterService = emojiCounterService;
        }

        [GroupCommand]
        [Description("Získaní přehledu o počtu použití za den")]
        public Task PerDayCommand(CommandContext ctx)
        {
            return Display(ctx, false);
        }

        [Command("total")]
        [Description("Získaní přehledu o počtu použití za celou donu")]
        public Task TotalCommand(CommandContext ctx)
        {
            return Display(ctx, true);
        }

        private async Task Display(CommandContext ctx, bool total)
        {
            IEnumerable<CountedEmoji> results = await _emojiCounterService.ListAsync();
            await ctx.RespondAsync("**Statistika používání custom emotes**");

            StringBuilder builder = new StringBuilder();
            builder.Append("\n");

            int emojisAppended = 0;
            const int chunkSize = 30;

            foreach (CountedEmoji result in results)
            {
                DiscordEmoji emoji = await ctx.Guild.GetEmojiAsync(result.Id);

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

            string remaining = builder.ToString();
            if (!string.IsNullOrEmpty(remaining.Trim()))
            {
                await ctx.RespondAsync(builder.ToString());
            }
            builder.Clear();
        }
    }
}
