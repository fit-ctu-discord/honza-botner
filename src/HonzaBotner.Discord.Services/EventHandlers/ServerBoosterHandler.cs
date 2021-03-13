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
    public class ServerBoosterHandler : IEventHandler<GuildUpdateEventArgs>
    {
        private readonly ILogger<ServerBoosterHandler> _logger;
        private readonly CommonCommandOptions _commonOptions;

        public ServerBoosterHandler(ILogger<ServerBoosterHandler> logger, IOptions<CommonCommandOptions> commonOptions)
        {
            _logger = logger;
            _commonOptions = commonOptions.Value;
        }

        public async Task<EventHandlerResult> Handle(GuildUpdateEventArgs eventArgs)
        {
            if ((eventArgs.GuildBefore.PremiumSubscriptionCount ?? 0) <=
                (eventArgs.GuildAfter.PremiumSubscriptionCount ?? 0))
                return EventHandlerResult.Continue;

            HashSet<ulong> subscribersBefore = new();
            HashSet<ulong> subscribersAfter = new();

            foreach ((ulong id, DiscordMember member) in eventArgs.GuildBefore.Members)
            {
                if (member.PremiumSince.HasValue)
                {
                    subscribersBefore.Add(id);
                }
            }

            foreach ((ulong id, DiscordMember member) in eventArgs.GuildAfter.Members)
            {
                if (member.PremiumSince.HasValue)
                {
                    subscribersAfter.Add(id);
                }
            }

            HashSet<ulong> newSubscribers = subscribersAfter.Except(subscribersBefore).ToHashSet();

            if (!newSubscribers.Any())
            {
                _logger.LogError("No new subscriber even while count after is greater than count before");
                return EventHandlerResult.Continue;
            }

            foreach (ulong newSubscriberId in newSubscribers)
            {
                try
                {
                    DiscordMember newSubscriber = await eventArgs.GuildAfter.GetMemberAsync(newSubscriberId);
                    DiscordChannel channel = eventArgs.GuildAfter.GetChannel(_commonOptions.GentlemenChannelId);
                    await channel.SendFileAsync(
                        _commonOptions.GentlemenFilePath,
                        $"Šálí mě snad zrak? Další gentleman! Přidal se k nám <@{newSubscriber.Id}>!"
                    );
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Couldn't get new subscriber or send welcome message");
                }
            }

            return EventHandlerResult.Continue;
        }
    }
}
