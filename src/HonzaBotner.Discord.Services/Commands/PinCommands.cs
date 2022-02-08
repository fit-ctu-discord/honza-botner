using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using HonzaBotner.Discord.Services.Attributes;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("pin")]
    [Description("Commands to manage pins on server")]
    [RequireGuild]
    [RequireMod]
    public class PinCommands : BaseCommandModule
    {
        [Group("delete")]
        [Aliases("erase", "remove")]
        [Description("Deletes pins on this server or in specific channel.")]
        public class DeletePins : BaseCommandModule
        {
            private readonly ILogger<DeletePins> _logger;
            private readonly PinOptions _pinOptions;
            private readonly DiscordWrapper _discordWrapper;

            public DeletePins(
                ILogger<DeletePins> logger,
                IOptions<PinOptions> options,
                DiscordWrapper wrapper
            )
            {
                _logger = logger;
                _pinOptions = options.Value;
                _discordWrapper = wrapper;
            }

            [Command("all")]
            [Description("Unpins all messages in all text channels on this server.")]
            public async Task DeleteAllAsync(CommandContext ctx)
            {
                var emoji = DiscordEmoji.FromName(ctx.Client, ":ok_hand:");
                DiscordMessage reactMessage = await ctx.Channel.SendMessageAsync(
                    "To approve deleting all temporary pings on this server, " +
                    $"react with {emoji} within 15 seconds."
                );

                await reactMessage.CreateReactionAsync(emoji);
                InteractivityResult<MessageReactionAddEventArgs> result =
                    await reactMessage.WaitForReactionAsync(ctx.User, emoji, TimeSpan.FromSeconds(15));

                if (result.TimedOut) return;

                await ctx.TriggerTypingAsync();

                var permanentPinEmoji = DiscordEmoji.FromName(_discordWrapper.Client, _pinOptions.PermanentPinName);
                var lockPinEmoji = DiscordEmoji.FromName(_discordWrapper.Client, _pinOptions.LockEmojiName);
                IReadOnlyDictionary<ulong, int> roleToScore = GetRoleToScore();
                var channelTasks = new List<Task>();

                foreach (DiscordChannel channel in ctx.Guild.Channels.Values)
                {
                    if (channel.Type is not ChannelType.Text && channel.Type is not ChannelType.News)
                    {
                        continue;
                    }

                    channelTasks.Add(DeletePinsInChannelAsync(channel, permanentPinEmoji, lockPinEmoji, roleToScore));
                }

                await Task.WhenAll(channelTasks);

                try
                {
                    await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(_discordWrapper.Client, ":+1:"));
                    await ctx.RespondAsync("Removed all temporary pins");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not add reaction or respond to {Message}", ctx.Message);
                }
            }

            [GroupCommand]
            [Command("channel")]
            [Description("Unpins messages in specified text channel.")]
            public async Task DeleteInChannelAsync(CommandContext ctx, DiscordChannel channel)
            {
                DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":ok_hand:");
                DiscordMessage reactMessage = await ctx.Channel.SendMessageAsync(
                    $"To approve deleting all pings in channel {channel.Mention}, " +
                    $"react with {emoji} within 15 seconds."
                );

                await reactMessage.CreateReactionAsync(emoji);
                InteractivityResult<MessageReactionAddEventArgs> result =
                    await reactMessage.WaitForReactionAsync(ctx.User, emoji, TimeSpan.FromSeconds(15));

                if (result.TimedOut) return;

                if (channel.Type is not ChannelType.Text && channel.Type is not ChannelType.News)
                {
                    await ctx.RespondAsync("You can use this command only on text channels");
                    return;
                }

                var permanentPinEmoji = DiscordEmoji.FromName(_discordWrapper.Client, _pinOptions.PermanentPinName);
                var lockPinEmoji = DiscordEmoji.FromName(_discordWrapper.Client, _pinOptions.LockEmojiName);
                IReadOnlyDictionary<ulong, int> roleToScore = GetRoleToScore();

                await DeletePinsInChannelAsync(channel, permanentPinEmoji, lockPinEmoji, roleToScore);


                try
                {
                    await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(_discordWrapper.Client, ":+1:"));
                    await ctx.RespondAsync($"Removed temporary pins in the channel");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not add reaction or respond to {Message}", ctx.Message);
                }
            }

            private async Task DeletePinsInChannelAsync(
                DiscordChannel channel,
                DiscordEmoji permanentEmoji,
                DiscordEmoji lockEmoji,
                IReadOnlyDictionary<ulong, int> roleToScore
            )
            {
                IReadOnlyList<DiscordMessage> messages = await channel.GetPinnedMessagesAsync();
                foreach (DiscordMessage message in messages)
                {
                    int score = 0;
                    IReadOnlyList<DiscordUser> reactions =
                        await message.GetReactionsAsync(permanentEmoji, _pinOptions.Treshold);

                    if (reactions.Count == _pinOptions.Treshold) continue;

                    foreach (DiscordUser user in reactions)
                    {
                        int maxRoleScore = 1;

                        DiscordMember? member = null;
                        try
                        {
                            member = await channel.Guild.GetMemberAsync(user.Id);
                        }
                        catch (Exception e)
                        {
                            _logger.LogInformation(e, "Could not initialize Guild member {MemberId}", user.Id);
                        }

                        if (member == null) continue;

                        foreach (DiscordRole role in member.Roles)
                        {
                            if (!roleToScore.ContainsKey(role.Id))
                            {
                                continue;
                            }

                            if (maxRoleScore < roleToScore[role.Id])
                            {
                                maxRoleScore = roleToScore[role.Id];
                            }
                        }

                        score += maxRoleScore;
                    }

                    // Do not delete anything.
                    if (score >= _pinOptions.Treshold)
                    {
                        continue;
                    }

                    try
                    {
                        await message.CreateReactionAsync(lockEmoji);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e,
                            "Could not create a lock emoji reaction for message {Message} in channel {Channel}",
                            message.Id,
                            channel.Id
                        );
                        await message.UnpinAsync();
                    }
                }
            }

            private IReadOnlyDictionary<ulong, int> GetRoleToScore()
            {
                var roleToScore = new Dictionary<ulong, int>();
                foreach ((string? key, int value) in _pinOptions.RoleToWeightMapping)
                {
                    try
                    {
                        ulong roleId = ulong.Parse(key);
                        roleToScore.Add(roleId, value);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to parse role id {Key}", key);
                    }
                }

                return roleToScore;
            }
        }
    }
}
