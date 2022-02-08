using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public PinHandler(
            IOptions<PinOptions> pinOptions,
            DiscordWrapper discordWrapper,
            ILogger<PinHandler> logger,
            IOptions<CommonCommandOptions> options
        )
        {
            _pinOptions = pinOptions.Value;
            _discordWrapper = discordWrapper;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<EventHandlerResult> Handle(MessageReactionAddEventArgs eventArgs)
        {
            if (eventArgs.Channel.IsPrivate)
            {
                return EventHandlerResult.Continue;
            }

            DiscordEmoji temporaryPinEmoji;
            DiscordEmoji permanentPinEmoji;
            DiscordEmoji lockEmoji;
            DiscordEmoji pinEmoji;

            try
            {
                temporaryPinEmoji = DiscordEmoji.FromName(_discordWrapper.Client, _pinOptions.TemporaryPinName);
                permanentPinEmoji = DiscordEmoji.FromName(_discordWrapper.Client, _pinOptions.PermanentPinName);
                lockEmoji = DiscordEmoji.FromName(_discordWrapper.Client, _pinOptions.LockEmojiName);
            }
            catch (ArgumentException e)
            {
                _logger.LogError("Failed to create discord emoji from name {EmojiName}", e.ParamName);
                return EventHandlerResult.Continue;
            }

            // Unpinning the message - mod or bot.
            if (eventArgs.Emoji.Equals(lockEmoji))
            {
                if (!eventArgs.Message.Pinned)
                {
                    return EventHandlerResult.Continue;
                }

                DiscordMember member = await eventArgs.Guild.GetMemberAsync(eventArgs.User.Id);
                if (member.Roles.Contains(eventArgs.Guild.GetRole(_options.ModRoleId)) ||
                    member.Id == eventArgs.Guild.CurrentMember.Id)
                {
                    await eventArgs.Message.UnpinAsync();
                }

                return EventHandlerResult.Continue;
            }

            // If pinned, nothing new will happen.
            if (eventArgs.Message.Pinned) return EventHandlerResult.Continue;

            if (eventArgs.Emoji.Equals(temporaryPinEmoji))
            {
                pinEmoji = temporaryPinEmoji;
            }
            else if (eventArgs.Emoji.Equals(permanentPinEmoji))
            {
                pinEmoji = permanentPinEmoji;
            }
            else
            {
                return EventHandlerResult.Continue;
            }

            IReadOnlyList<DiscordUser> lockedReactions = await eventArgs.Message.GetReactionsAsync(lockEmoji);

            foreach (DiscordUser user in lockedReactions)
            {
                DiscordMember member = await eventArgs.Guild.GetMemberAsync(user.Id);

                // Mod or bot locked the message, no need to process further.
                if (member.Roles.Contains(eventArgs.Guild.GetRole(_options.ModRoleId)) ||
                    member.Id == eventArgs.Guild.CurrentMember.Id)
                {
                    return EventHandlerResult.Continue;
                }
            }

            IReadOnlyList<DiscordUser> reactions = await eventArgs.Message.GetReactionsAsync(pinEmoji);

            // Fast heuristic to check if threshold is passed.
            if (reactions.Count >= _pinOptions.Treshold)
            {
                await eventArgs.Message.PinAsync();
                return EventHandlerResult.Continue;
            }

            // Build roleToScore dictionary mapping.
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

            // Compute pin score.
            int score = 0;
            foreach (DiscordUser user in reactions)
            {
                int maxRoleScore = 1;

                DiscordMember? member = null;
                try
                {
                    member = await eventArgs.Guild.GetMemberAsync(user.Id);
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

            if (score >= _pinOptions.Treshold)
            {
                await eventArgs.Message.PinAsync();
            }

            return EventHandlerResult.Continue;
        }
    }
}
