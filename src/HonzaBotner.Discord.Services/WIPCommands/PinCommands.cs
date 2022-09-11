using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chronic.Core.System;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Commands;

public class PinCommands : ApplicationCommandModule
{
    private readonly ILogger<PinCommands> _logger;
    private readonly PinOptions _pinOptions;
    private readonly DiscordWrapper _discordWrapper;
    private readonly IReadOnlyDictionary<ulong, int> _roleToScore;

    public PinCommands(
        ILogger<PinCommands> logger,
        IOptions<PinOptions> options,
        DiscordWrapper wrapper
    )
    {
        _logger = logger;
        _pinOptions = options.Value;
        _discordWrapper = wrapper;
        _roleToScore = GetRoleToScore();
    }

    [SlashCommand("delete-pins", "Unpins messages pinned with temporary pins")]
    [SlashCommandPermissions(Permissions.ManageChannels)]
    public async Task DeleteAllPinsCommandAsync(
        InteractionContext ctx,
        [Option("everywhere", "Do it everywhere or just in this channel? Default: false")]
        bool everywhere = false)
    {
        string customId = $"unpin + {ctx.User.Id} + {ctx.Channel.Id}";
        await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
            .WithContent("Do you really want to unpin messages in this " + (everywhere ? "server?" : "channel?"))
            .AddComponents(new DiscordButtonComponent(ButtonStyle.Danger, customId, "Do it!"))
            .AsEphemeral(true));

        var interactivity = ctx.Client.GetInteractivity();
        var result = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), customId,
            TimeSpan.FromSeconds(30));

        if (result.TimedOut) return;

        var permanentPinEmoji = DiscordEmoji.FromName(_discordWrapper.Client, _pinOptions.PermanentPinName);
        var lockPinEmoji = DiscordEmoji.FromName(_discordWrapper.Client, _pinOptions.LockEmojiName);
        var channelTasks = new List<Task>();

        ctx.Guild.Channels.ForEach(pair =>
        {
            if (pair.Value.Type is ChannelType.Category or ChannelType.Group or ChannelType.Stage or ChannelType.Unknown or ChannelType.Voice) return;
            if (!everywhere && pair.Key != ctx.Channel.Id) return;
            channelTasks.Add(DeletePinsInChannelAsync(pair.Value, permanentPinEmoji, lockPinEmoji));
        });

        await Task.WhenAll(channelTasks);
        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Messages successfully unpinned"));
    }

    private async Task DeletePinsInChannelAsync(
        DiscordChannel channel,
        DiscordEmoji permanentEmoji,
        DiscordEmoji lockEmoji)
    {
        IReadOnlyList<DiscordMessage> messages = await channel.GetPinnedMessagesAsync();
        foreach (DiscordMessage message in messages)
        {
            int score = 0;
            IReadOnlyList<DiscordUser> reactions =
                await message.GetReactionsAsync(permanentEmoji, _pinOptions.Threshold);

            if (reactions.Count >= _pinOptions.Threshold) continue;

            foreach (DiscordUser user in reactions)
            {
                int maxRoleScore = 1;

                DiscordMember? member = await channel.Guild.GetMemberAsync(user.Id);
                if (member is not null)
                {
                    foreach (DiscordRole role in member.Roles)
                    {
                        if (!_roleToScore.ContainsKey(role.Id))
                        {
                            continue;
                        }

                        if (maxRoleScore < _roleToScore[role.Id])
                        {
                            maxRoleScore = _roleToScore[role.Id];
                        }
                    }
                }

                score += maxRoleScore;
            }

            // Do not delete anything.
            if (score >= _pinOptions.Threshold)
            {
                continue;
            }

            try
            {
                await message.CreateReactionAsync(lockEmoji);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e,
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
