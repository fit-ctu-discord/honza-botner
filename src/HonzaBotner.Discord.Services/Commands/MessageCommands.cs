using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using HonzaBotner.Discord.Services.Extensions;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Commands;

[SlashCommandGroup("message", "Commands to interact with messages.")]
[SlashCommandPermissions(Permissions.ManageMessages)]
[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public class MessageCommands : ApplicationCommandModule
{
    private readonly ILogger<MessageCommands> _logger;
    private readonly IRoleBindingsService _roleBindingsService;

    public MessageCommands(ILogger<MessageCommands> logger, IRoleBindingsService roleBindingsService)
    {
        _logger = logger;
        _roleBindingsService = roleBindingsService;
    }

    [SlashCommand("send", "Sends a text message to the specified channel.")]
    public async Task SendMessageCommandAsync(
        InteractionContext ctx,
        [Option("channel", "Target channel for the message")] DiscordChannel channel,
        [Option("new-message", "Link to the message with content you want sent")] string link,
        [Option("mention", "Should the message include mentions? Default: false")] bool mention = false)
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
    public async Task EditMessageCommandAsync(
        InteractionContext ctx,
        [Option("old-message", "Link to the message you want to edit")] string originalUrl,
        [Option("new-message", "Link to a message with new content")] string newUrl,
        [Option("mention", "Should all mentions be included?")] bool mention = false)
    {
        DiscordMessage? oldMessage = await DiscordHelper.FindMessageFromLink(ctx.Guild, originalUrl);
        DiscordMessage? newMessage = await DiscordHelper.FindMessageFromLink(ctx.Guild, newUrl);

        if (oldMessage is null || newMessage is null)
        {
            await ctx.CreateResponseAsync("Could not resolve one of the provided messages");
            return;
        }

        if (!oldMessage.Author.IsCurrent)
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

    [SlashCommand("react", "Reacts to a message as this bot.")]
    public async Task ReactToMessageCommandAsync(
        InteractionContext ctx,
        [Option("message", "Link to the message")] string url
        )
    {
        DiscordGuild guild = ctx.Guild;
        DiscordMessage? oldMessage = await DiscordHelper.FindMessageFromLink(guild, url);

        if (oldMessage is null)
        {
            await ctx.CreateResponseAsync("Could not find message to react to.");
            return;
        }

        await ctx.CreateResponseAsync("React to this message with reactions you want to add");
        var reactionCatch = await ctx.GetOriginalResponseAsync();
        var interactivity = ctx.Client.GetInteractivity();
        var response = await interactivity
            .WaitForReactionAsync(reactionCatch, ctx.User, TimeSpan.FromMinutes(2));

        while (!response.TimedOut)
        {
            try
            {
                await oldMessage.CreateReactionAsync(response.Result.Emoji);
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder()
                        .WithContent("Reacted with " + response.Result.Emoji + "\nReact with more to add more"));
            }
            catch (BadRequestException)
            {
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder()
                        .WithContent("Bot cannot react with provided emoji. Is it universal/from this server?"));
            }
            catch (UnauthorizedException)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Too many reactions"));
                return;
            }

            response = await interactivity.WaitForReactionAsync(reactionCatch, ctx.User, TimeSpan.FromMinutes(2));
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No more reactions added"));
    }

    [SlashCommand("bind", "Bind or unbind roles to specified message and reaction")]
    public async Task BindCommandAsync(
        InteractionContext ctx,
        [Option("message", "Link to modified message")] string url,
        [Option("roles", "Mention roles you want to (un)bind")] string roles,
        [Choice("add", "add")]
        [Choice("remove", "remove")]
        [Option("action", "Add new binding or remove existing?")] string action)
    {
        DiscordMessage? message = await DiscordHelper.FindMessageFromLink(ctx.Guild, url);
        if (message == null)
        {
            await ctx.CreateResponseAsync("Unable to find message with provided link", true);
            return;
        }

        ulong channelId = message.ChannelId;
        ulong messageId = message.Id;

        await ctx.CreateResponseAsync("React to this message with emoji you want to (un)bind");
        var interactivity = ctx.Client.GetInteractivity();
        var response =
            await interactivity.WaitForReactionAsync(ctx.GetOriginalResponseAsync().Result, ctx.User,
                TimeSpan.FromMinutes(2));

        if (response.TimedOut)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Timed out, write command again"));
            return;
        }

        try
        {
            switch (action)
            {
                case "add":
                    await message.CreateReactionAsync(response.Result.Emoji);
                    await _roleBindingsService.AddBindingsAsync(channelId, messageId, response.Result.Emoji.Name,
                        ctx.ResolvedRoleMentions.Select(r => r.Id).ToHashSet());
                    break;
                case "remove":
                    bool someRemained = await _roleBindingsService.RemoveBindingsAsync(channelId, messageId,
                        response.Result.Emoji.Name,
                        ctx.ResolvedRoleMentions.Select(r => r.Id).ToHashSet());
                    if (!someRemained) await message.DeleteReactionsEmojiAsync(response.Result.Emoji);
                    break;
            }

            await ctx.FollowUpAsync(
                new DiscordFollowupMessageBuilder().WithContent("Successfully " + action + "ed role binding"));
        }
        catch (BadRequestException)
        {
            await ctx.FollowUpAsync(
                new DiscordFollowupMessageBuilder().WithContent("Cannot use provided emote."));
        }
        catch (Exception e)
        {
            await ctx.FollowUpAsync(
                new DiscordFollowupMessageBuilder().WithContent("Error occured. Refer to log for more info."));
            _logger.LogError(e, "Error occured during command {Action} bind of roles to message", action);
        }
    }
}
