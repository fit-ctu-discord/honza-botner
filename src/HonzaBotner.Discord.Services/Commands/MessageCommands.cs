using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Extensions;
using HonzaBotner.Discord.Services.Attributes;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("message")]
    [Description("Commands to interact with messages.")]
    [Hidden]
    [RequireMod]
    public class MessageCommands : BaseCommandModule
    {
        private readonly ILogger<MemberCommands> _logger;

        public MessageCommands(ILogger<MemberCommands> logger)
        {
            _logger = logger;
        }

        [Command("send")]
        [Description("Sends a text message to the specified channel.")]
        public async Task SendMessage(CommandContext ctx,
            [Description("Channel to send a text message to.")]
            DiscordChannel channel,
            [RemainingText, Description("Text of the message to send.")]
            string text)
        {
            await channel.SendMessageAsync(text.RemoveDiscordMentions(ctx.Guild));
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":+1:"));
        }

        [Command("edit")]
        [Description("Edits previously send text message authored by this bot.")]
        public async Task EditMessage(CommandContext ctx,
            [Description("URL of the message.")] string url,
            [RemainingText, Description("New text of the message.")]
            string newText)
        {
            DiscordMessage? oldMessage = await DiscordHelper.FindMessageFromLink(ctx.Message.Channel.Guild, url);

            if (oldMessage == null)
            {
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":-1:"));
                return;
            }

            await oldMessage.ModifyAsync(newText);
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":+1:"));
        }

        [Command("react")]
        [Description("Reacts to a message as this bot.")]
        public async Task ReactToMessage(CommandContext ctx,
            [Description("URL of the message.")] string url,
            [Description("Emojis to react with.")] params DiscordEmoji[] emojis)
        {
            DiscordMessage? oldMessage = await DiscordHelper.FindMessageFromLink(ctx.Message.Channel.Guild, url);

            if (oldMessage == null)
            {
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":-1:"));
                return;
            }

            foreach (DiscordEmoji emoji in emojis)
            {
                try
                {
                    await oldMessage.CreateReactionAsync(emoji);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Could't react with emoji {0}.", emoji);
                    await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":bug:"));
                    return;
                }
            }

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":+1:"));
        }
    }
}
