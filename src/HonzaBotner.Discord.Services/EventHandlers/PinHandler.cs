using System;
using System.Collections.Generic;
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

        public PinHandler(IOptions<PinOptions> pinOptions, DiscordWrapper discordWrapper, ILogger<PinHandler> logger)
        {
            _pinOptions = pinOptions.Value;
            _discordWrapper = discordWrapper;
            _logger = logger;
        }

        public async Task<EventHandlerResult> Handle(MessageReactionAddEventArgs eventArgs)
        {
            if (eventArgs.Message.Pinned) return EventHandlerResult.Continue;

            try
            {
                DiscordEmoji pinEmoji = DiscordEmoji.FromName(_discordWrapper.Client, _pinOptions.EmojiName);

                if (eventArgs.Emoji.Equals(pinEmoji))
                {
                    IReadOnlyList<DiscordUser> reactions = await eventArgs.Message.GetReactionsAsync(pinEmoji);

                    if (reactions.Count >= _pinOptions.Treshold)
                    {
                        await eventArgs.Message.PinAsync();
                    }
                    else
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
                    }
                }
            }
            catch
            {
                _logger.LogError("Failed to create discord emoji from name {EmojiName}", _pinOptions.EmojiName);
            }

            return EventHandlerResult.Continue;
        }
    }
}
