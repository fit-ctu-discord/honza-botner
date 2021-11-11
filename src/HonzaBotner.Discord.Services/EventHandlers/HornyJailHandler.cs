using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Services.EventHandlers.Helpers;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers
{
    public class HornyJailHandler : WelcomeMessageHandler, IEventHandler<GuildMemberUpdateEventArgs>
    {
        private readonly ILogger<HornyJailHandler> _logger;
        private readonly CommonCommandOptions _commonOptions;

        public HornyJailHandler(ILogger<HornyJailHandler> logger, IOptions<CommonCommandOptions> commonOptions)
        {
            _logger = logger;
            _commonOptions = commonOptions.Value;
        }

        public async Task<EventHandlerResult> Handle(GuildMemberUpdateEventArgs eventArgs)
        {
            return await HandleAddedRole(
                eventArgs,
                _commonOptions.HornyJailRoleId,
                _commonOptions.HornyJailChannelId,
                _commonOptions.HornyJailFilePath,
                $"Ajaj, <@{eventArgs.Member.Id}>, další dirty coder!",
                _logger
            );
        }
    }
}
