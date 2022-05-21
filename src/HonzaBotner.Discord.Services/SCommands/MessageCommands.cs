using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HonzaBotner.Discord.Services.Extensions;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.SCommands;

[SlashCommandGroup("message", "Commands to interact with messages.")]
[GuildOnly]
[SlashCommandPermissions(Permissions.ManageMessages)]
public class MessageCommands : ApplicationCommandModule
{
    private readonly ILogger<MessageCommands> _logger;

    public MessageCommands(ILogger<MessageCommands> logger)
    {
        _logger = logger;
    }

    [SlashCommand("send", "Sends a text message to the specified channel.")]
    public async Task SendMessage(
        InteractionContext ctx,
        [Option("channel", "target channel for the message")] DiscordChannel channel,
        [Option("messageLink", "Link to the message with new content")] string link,
        [Option("mention", "Should all mentions be included?")] bool mention = false)
    {
        DiscordMessage? messageToSend = await DiscordHelper.FindMessageFromLink(ctx.Guild, link);

        if (messageToSend is not null)
        {
            try
            {
                string valueToSend = messageToSend.Content;
                if (!mention)
                {
                    valueToSend = valueToSend.RemoveDiscordMentions(ctx.Guild);
                }

                await channel.SendMessageAsync(valueToSend);
                await ctx.CreateResponseAsync("Message sent");
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error during sending bot message");
                await ctx.CreateResponseAsync("Error occured during message send, see log for more information");
            }
            return;
        }

        await ctx.CreateResponseAsync("Could not find linked message, does the bot have access to that channel?");
    }

    [SlashCommand("edit", "Edit previously sent text message authored by this bot.")]
    public async Task EditMessage(
        InteractionContext ctx,
        [Option("originalLink", "Link to message you want to edit")] string originalUrl,
        [Option("newLink", "Link to the message with new content")] string newUrl,
        [Option("mention", "Should all mentions be included?")] bool mention = false)
    {
        DiscordMessage? oldMessage = await DiscordHelper.FindMessageFromLink(ctx.Guild, originalUrl);
        DiscordMessage? newMessage = await DiscordHelper.FindMessageFromLink(ctx.Guild, newUrl);

        if (oldMessage is null || newMessage is null)
        {
            await ctx.CreateResponseAsync("Could not resolve one of the provided messages");
            return;
        }

        if (oldMessage.Author != ctx.Client.CurrentUser)
        {
            await ctx.CreateResponseAsync("Can not edit messages which were not sent by this bot (duh)");
            return;
        }

        string newText = newMessage.Content;
        if (!mention)
        {
            newText = newText.RemoveDiscordMentions(ctx.Guild);
        }

        try
        {
            await oldMessage.ModifyAsync(newText);
            await ctx.CreateResponseAsync("Message successfully edited.");
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Could not edit message in {OldMessageChannel}", oldMessage.Channel.Name);
            await ctx.CreateResponseAsync("Error occured during message edit, see log for more information");
        }
    }

    // TODO: From here further down

    [Command("react")]
    [Description("Reacts to a message as this bot.")]
    public async Task ReactToMessage(CommandContext ctx,
        [Description("URL of the message.")] string url,
        [Description("Emojis to react with.")] params DiscordEmoji[] emojis)
    {
        DiscordGuild guild = ctx.Guild;
        DiscordMessage? oldMessage = await DiscordHelper.FindMessageFromLink(guild, url);

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
            DiscordGuild guild = ctx.Guild;
            DiscordMessage? message = await DiscordHelper.FindMessageFromLink(guild, url);
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
    }
}
