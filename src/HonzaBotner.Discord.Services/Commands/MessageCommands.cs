using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using HonzaBotner.Discord.Extensions;
using HonzaBotner.Discord.Services.Attributes;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("message")]
    [Description("Commands to interact with messages.")]
    [RequireMod]
    public class MessageCommands : BaseCommandModule
    {
        private readonly ILogger<MessageCommands> _logger;

        public MessageCommands(ILogger<MessageCommands> logger)
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
            string valueToSend = text.RemoveDiscordMentions(ctx.Guild);

            if (text != valueToSend)
            {
                DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":ok_hand:");
                DiscordMessage reactMessage =
                    await ctx.Channel.SendMessageAsync(
                        $"To approve sending message with ping, react with {emoji}. (15 seconds timeout to send without pings.)");
                await reactMessage.CreateReactionAsync(emoji);
                InteractivityResult<MessageReactionAddEventArgs> result =
                    await reactMessage.WaitForReactionAsync(ctx.Member, emoji, TimeSpan.FromSeconds(15));

                if (!result.TimedOut)
                {
                    valueToSend = text;
                }
            }

            try
            {
                await channel.SendMessageAsync(valueToSend);
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":+1:"));
            }
            catch (Exception e)
            {
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":-1:"));
                _logger.LogWarning(e, "Failed to send message '{ValueToSend}'. It might have been empty", valueToSend);
            }
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
                    _logger.LogWarning(e, "Couldn't react with emoji {Emoji}", emoji);
                    await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":bug:"));
                    return;
                }
            }

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":+1:"));
        }

        [Group("bind")]
        [Description("Module used for binding roles to emoji reaction")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class RoleBindingsCommands : BaseCommandModule
        {
            private readonly IRoleBindingsService _roleBindingsService;
            private readonly ILogger<RoleBindingsCommands> _logger;

            public RoleBindingsCommands(IRoleBindingsService roleBindingsService, ILogger<RoleBindingsCommands> logger)
            {
                _roleBindingsService = roleBindingsService;
                _logger = logger;
            }

            [GroupCommand]
            [Description("Adds binding to message")]
            [Command("add")]
            public async Task AddBinding(CommandContext ctx, [Description("URL of the message.")] string url,
                [Description("Emoji to react with.")] DiscordEmoji emoji,
                [Description("Roles which will be toggled after reaction.")]
                params DiscordRole[] roles)
            {
                DiscordMessage? message = await DiscordHelper.FindMessageFromLink(ctx.Guild, url);
                if (message == null)
                {
                    throw new ArgumentOutOfRangeException($"Couldn't find message with link: {url}");
                }

                ulong channelId = message.ChannelId;
                ulong messageId = message.Id;


                await _roleBindingsService.AddBindingsAsync(channelId, messageId, emoji.Name,
                    roles.Select(r => r.Id).ToHashSet());
                try
                {
                    await message.CreateReactionAsync(emoji);
                    DiscordEmoji thumbsUp = DiscordEmoji.FromName(ctx.Client, ":+1:");
                    await ctx.Message.CreateReactionAsync(thumbsUp);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Couldn't add reaction for emoji: {EmojiName} on {Url}",
                        emoji.Name, url);
                    DiscordEmoji thumbsUp = DiscordEmoji.FromName(ctx.Client, ":-1:");
                    await ctx.Message.CreateReactionAsync(thumbsUp);
                }
            }

            [Command("remove")]
            [Description("Removes binding from message")]
            public async Task RemoveBinding(CommandContext ctx, [Description("URL of the message.")] string url,
                [Description("Emoji to react with.")] DiscordEmoji emoji,
                [Description("Roles which will be toggled after reaction")]
                params DiscordRole[] roles)
            {
                DiscordMessage? message = await DiscordHelper.FindMessageFromLink(ctx.Guild, url);
                if (message == null)
                {
                    throw new ArgumentOutOfRangeException($"Couldn't find message with link: {url}");
                }

                ulong channelId = message.ChannelId;
                ulong messageId = message.Id;

                bool someRemained = await _roleBindingsService.RemoveBindingsAsync(channelId, messageId, emoji.Name,
                    roles.Select(r => r.Id).ToHashSet());

                DiscordEmoji thumbsUp = DiscordEmoji.FromName(ctx.Client, ":+1:");
                await ctx.Message.CreateReactionAsync(thumbsUp);

                if (!someRemained)
                {
                    try
                    {
                        // await message.DeleteReactionsEmojiAsync(emoji);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Couldn't add reaction for emoji: {EmojiName} on {Url}",
                            emoji.Name, url);
                    }
                }
            }

/*
            [Command("dump")]
            public async Task Dump(CommandContext ctx, [Description("URL of the message.")] string url)
            {
                DiscordMessage? message = await DiscordHelper.FindMessageFromLink(ctx.Guild, url);
                if (message == null)
                {
                    throw new ArgumentOutOfRangeException($"Couldn't find message with link: {url}");
                }

                ulong channelId = message.ChannelId;
                ulong messageId = message.Id;

                IList<ulong> roles = await _roleBindingsService.FindMappingAsync(channelId, messageId);


                // TODO: Pretty print of associated bindings with message
                await ctx.Message.RespondAsync($"TODO: {roles.Count}");
            }*/
        }
    }
}
