using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Reactions
{
    public class PinHandler : IReactionHandler
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

        public async Task<IReactionHandler.Result> HandleAddAsync(MessageReactionAddEventArgs eventArgs)
        {
            if (eventArgs.Message.Pinned) return IReactionHandler.Result.Continue;

            try
            {
                DiscordEmoji pinEmoji = DiscordEmoji.FromName(_discordWrapper.Client, _pinOptions.EmojiName);

                if (eventArgs.Emoji.Equals(pinEmoji))
                {
                    var reactions = await eventArgs.Message.GetReactionsAsync(pinEmoji);

                    if (reactions.Count >= _pinOptions.Treshold)
                    {
                        await eventArgs.Message.PinAsync();
                    }
                    else
                    {
                        Dictionary<ulong, int> roleToScore = new Dictionary<ulong, int>();
                        foreach (var keyValuePair in _pinOptions.RoleToWeightMapping)
                        {
                            try
                            {
                                ulong roleId = ulong.Parse(keyValuePair.Key);
                                roleToScore.Add(roleId, keyValuePair.Value);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "Failed to parse role id {0}.", keyValuePair.Key);
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
                _logger.LogError("Failed to create discord emoji from name {0}.", _pinOptions.EmojiName);
            }

            return IReactionHandler.Result.Continue;
        }
    }
}
