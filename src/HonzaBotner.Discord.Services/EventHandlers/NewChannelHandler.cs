using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers
{
    public class NewChannelHandler : IEventHandler<ChannelCreateEventArgs>
    {
        private readonly ILogger<NewChannelHandler> _logger;
        private readonly CommonCommandOptions _commonOptions;

        public NewChannelHandler(ILogger<NewChannelHandler> logger, IOptions<CommonCommandOptions> commonOptions)
        {
            _logger = logger;
            _commonOptions = commonOptions.Value;
        }

        public async Task<EventHandlerResult> Handle(ChannelCreateEventArgs eventArgs)
        {
            try
            {
                DiscordRole muteRole = eventArgs.Guild.GetRole(_commonOptions.MuteRoleId);
                await eventArgs.Channel.AddOverwriteAsync(
                    muteRole,
                    deny: Permissions.SendMessages | Permissions.AddReactions | Permissions.SendTtsMessages
                );
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Couldn't add mute role override");
            }

            try
            {
                DiscordRole botRole = eventArgs.Guild.GetRole(_commonOptions.BotRoleId);
                await eventArgs.Channel.AddOverwriteAsync(
                    botRole,
                    Permissions.AccessChannels | Permissions.SendMessages | Permissions.UseVoice
                );
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Couldn't add mod role override");
            }

            return EventHandlerResult.Continue;
        }
    }
}
