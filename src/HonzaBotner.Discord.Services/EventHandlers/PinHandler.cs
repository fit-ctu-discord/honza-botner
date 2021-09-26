using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers
{
    public class PinHandler : IEventHandler<MessageReactionAddEventArgs>
    {
        private readonly PinOptions _pinOptions;
        private readonly DiscordWrapper _discordWrapper;
        private readonly ILogger<PinHandler> _logger;
        private readonly CommonCommandOptions _options;

        public PinHandler(IOptions<PinOptions> pinOptions, DiscordWrapper discordWrapper, ILogger<PinHandler> logger,
            IOptions<CommonCommandOptions> options)
        {
            _pinOptions = pinOptions.Value;
            _discordWrapper = discordWrapper;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<EventHandlerResult> Handle(MessageReactionAddEventArgs eventArgs)
        {
            DiscordEmoji tempPinEmoji;
            DiscordEmoji permPinEmoji;
            DiscordEmoji lockEmoji;
            DiscordEmoji pinEmoji;

            if (eventArgs.Channel.IsPrivate)
            {
                return EventHandlerResult.Continue;
            }

            try
            {
                tempPinEmoji = DiscordEmoji.FromName(_discordWrapper.Client, _pinOptions.TemporaryPinName);
                permPinEmoji = DiscordEmoji.FromName(_discordWrapper.Client, _pinOptions.PermanentPinName);
                lockEmoji = DiscordEmoji.FromName(_discordWrapper.Client, _pinOptions.LockEmojiName);
            }
            catch (ArgumentException e)
            {
                _logger.LogError("Failed to create discord emoji from name {EmojiName}", e.ParamName);
                return EventHandlerResult.Continue;
            }
            
            if (eventArgs.Emoji.Equals(tempPinEmoji))
            {
                pinEmoji = tempPinEmoji;
            }
            else if (eventArgs.Emoji.Equals(permPinEmoji))
            {
                pinEmoji = permPinEmoji;
            }
            else if (eventArgs.Emoji.Equals(lockEmoji))
            {
                DiscordMember member = await eventArgs.Guild.GetMemberAsync(eventArgs.User.Id);
                if (member.Roles.Contains(eventArgs.Guild.GetRole(_options?.ModRoleId ?? 0)))
                {
                    await eventArgs.Message.UnpinAsync();
                }
                return EventHandlerResult.Continue;
            }
            else
            {
                return EventHandlerResult.Continue;
            }
            
            if (eventArgs.Message.Pinned) return EventHandlerResult.Continue;
            
            IReadOnlyList<DiscordUser> lockedReactions = await eventArgs.Message.GetReactionsAsync(lockEmoji);
            foreach (DiscordUser user in lockedReactions)
            {
                DiscordMember member = await eventArgs.Guild.GetMemberAsync(user.Id);
                if (member.Roles.Contains(eventArgs.Guild.GetRole(_options?.ModRoleId ?? 0)))
                {
                    return EventHandlerResult.Continue;
                }
            }

            IReadOnlyList<DiscordUser> reactions = await eventArgs.Message.GetReactionsAsync(pinEmoji);

            if (reactions.Count >= _pinOptions.Treshold)
            {
                await eventArgs.Message.PinAsync();
                return EventHandlerResult.Continue;
            }
            
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
            
            int score = 0;
            foreach (DiscordUser user in reactions)
            {
                int maxRoleScore = 1;
                DiscordMember member = await eventArgs.Guild.GetMemberAsync(user.Id);
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

            if (score >= _pinOptions.Treshold)
            {
                await eventArgs.Message.PinAsync();
            }
            
            return EventHandlerResult.Continue;
        }
    }
}
