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
    [RequireMod]
    [RequireGuild]
    public class UnPinCommands : BaseCommandModule
    {
        [Group("delete")]
        [Description("Deletes pins on this server or in specific channel")]
        public class DeletePins : BaseCommandModule
        {

            private readonly ILogger<DeletePins> _logger;
            private readonly PinOptions _pinOptions;
            private readonly DiscordWrapper _discordWrapper;

            public DeletePins(ILogger<DeletePins> logger, IOptions<PinOptions> options, DiscordWrapper wrapper)
            {
                _logger = logger;
                _pinOptions = options.Value;
                _discordWrapper = wrapper;
            }
            
            [Command("all")]
            [Description("Unpins all messages in all text channels on this server")]
            public async Task DeleteAll(CommandContext ctx)
            {
                DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":ok_hand:");
                DiscordMessage reactMessage =
                    await ctx.Channel.SendMessageAsync(
                        "To approve deleting all temporary pings on this server, " +
                        $"react with {emoji} within 15 seconds.");
                await reactMessage.CreateReactionAsync(emoji);
                InteractivityResult<MessageReactionAddEventArgs> result =
                    await reactMessage.WaitForReactionAsync(ctx.User, emoji, TimeSpan.FromSeconds(15));

                if (result.TimedOut)
                {
                    return;
                }

                await ctx.TriggerTypingAsync();
                
                DiscordEmoji permPinEmoji = DiscordEmoji.FromName(_discordWrapper.Client, _pinOptions.PermanentPinName);
                Dictionary<ulong, int> roleToScore = GetRoleToScore();
                List<Task> channelTasks = new List<Task>();

                foreach (DiscordChannel channel in ctx.Guild.Channels.Values)
                {
                    if (channel.Type is not ChannelType.Text && channel.Type is not ChannelType.News)
                    {
                        continue;
                    }

                    channelTasks.Add(DeletePinsInChannel(channel, permPinEmoji, roleToScore));
                }

                await Task.WhenAll(channelTasks);

                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(_discordWrapper.Client, ":+1:"));
                await ctx.RespondAsync("Removed all temporary pins");
            }

            [Command("channel")]
            [GroupCommand]
            [Description("Unpins messages in specified text channel")]
            public async Task DeleteInChannel(CommandContext ctx, DiscordChannel channel)
            {
                DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":ok_hand:");
                DiscordMessage reactMessage =
                    await ctx.Channel.SendMessageAsync(
                        $"To approve deleting all pings in channel {channel.Mention}, react with {emoji} within 15 seconds.");
                await reactMessage.CreateReactionAsync(emoji);
                InteractivityResult<MessageReactionAddEventArgs> result =
                    await reactMessage.WaitForReactionAsync(ctx.User, emoji, TimeSpan.FromSeconds(15));

                if (result.TimedOut)
                {
                    return;
                }
                
                if (channel.Type is not ChannelType.Text && channel.Type is not ChannelType.News)
                {
                    await ctx.RespondAsync("You can use this command only on text channels");
                    return;
                }

                DiscordEmoji permPinEmoji =
                    DiscordEmoji.FromName(_discordWrapper.Client, _pinOptions.PermanentPinName);
                Dictionary<ulong, int> roleToScore = GetRoleToScore();

                await DeletePinsInChannel(channel, permPinEmoji, roleToScore);

                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(_discordWrapper.Client,":+1:"));

            }

            private async Task DeletePinsInChannel(DiscordChannel channel, DiscordEmoji permEmoji,
                Dictionary<ulong, int> roleToScore)
            {
                IReadOnlyList<DiscordMessage> messages = await channel.GetPinnedMessagesAsync();
                foreach (DiscordMessage message in messages)
                {
                    int score = 0;
                    IReadOnlyList<DiscordUser> reactions = await message.GetReactionsAsync(permEmoji, 10);
                    if (reactions.Count == 10) continue;
                    foreach (DiscordUser user in reactions)
                    {
                        int maxRoleScore = 1;
                        DiscordMember member = await channel.Guild.GetMemberAsync(user.Id);
                        foreach (DiscordRole role in member.Roles)
                        {
                            if (roleToScore.ContainsKey(role.Id))
                            {
                                if (maxRoleScore < roleToScore[role.Id])
                                {
                                    maxRoleScore = roleToScore[role.Id];
                                }
                            }
                        }

                        score += maxRoleScore;
                    }

                    if (score < _pinOptions.Treshold)
                    {
                        await message.UnpinAsync();
                    }
                }
            }

            private Dictionary<ulong, int> GetRoleToScore()
            {
                Dictionary<ulong, int> roleToScore = new Dictionary<ulong, int>();
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