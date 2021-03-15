using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Emzi0767;
using HonzaBotner.Discord.Extensions;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("fun")]
    [Description("Commands that bring fun!")]
    public class FunCommands : BaseCommandModule
    {
        private readonly ILogger<FunCommands> _logger;

        public FunCommands(ILogger<FunCommands> logger)
        {
            _logger = logger;
        }

        [Command("choose")]
        [Aliases("decide", "pick")]
        [Description("Decide a destiny.")]
        public async Task Choose(
            CommandContext ctx,
            [Description("Choices to decide from.")] [RemainingText]
            params string[] choices
        )
        {
            await ctx.TriggerTypingAsync();

            SecureRandom random = new();
            string selected = choices[random.Next(choices.Length)].RemoveDiscordMentions(ctx.Guild, _logger);

            try
            {
                if (selected.Trim().Length == 0)
                {
                    await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":-1:"));
                }
                else
                {
                    await ctx.Channel.SendMessageAsync(
                        (choices.Length == 1 ? "Jak prosté: " : "Vybral jsem: ")
                        + $"**{selected}**"
                    );
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Couldn't send a message");
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":-1:"));
            }
        }
    }
}
