using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Services.EventHandlers.Helpers;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers
{
    public class BoosterHandler : WelcomeMessageHandler, IEventHandler<GuildMemberUpdateEventArgs>
    {
        private readonly ILogger<BoosterHandler> _logger;
        private readonly CommonCommandOptions _commonOptions;

        public BoosterHandler(ILogger<BoosterHandler> logger, IOptions<CommonCommandOptions> commonOptions)
        {
            _logger = logger;
            _commonOptions = commonOptions.Value;
        }

        public async Task<EventHandlerResult> Handle(GuildMemberUpdateEventArgs eventArgs)
        {
            return await HandleAddedRole(
                eventArgs,
                _commonOptions.BoosterRoleId,
                _commonOptions.GentlemenChannelId,
                _commonOptions.GentlemenFilePath,
                $"Vítej v první třídě, <@{eventArgs.Member.Id}>!",
                _logger
            );
        }
    }
}
