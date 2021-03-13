using System;
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
    public class HornyJailHandler : IEventHandler<GuildMemberUpdateEventArgs>
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
            try
            {
                DiscordRole hornyJailRole = eventArgs.Guild.GetRole(_commonOptions.HornyJailRoleId);
                if (eventArgs.RolesBefore.Contains(hornyJailRole) || !eventArgs.RolesAfter.Contains(hornyJailRole))
                {
                    return EventHandlerResult.Continue;
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Couldn't get horny jail role");
            }

            try
            {
                DiscordChannel channel = eventArgs.Guild.GetChannel(_commonOptions.HornyJailChannelId);
                await channel.SendFileAsync(
                    _commonOptions.HornyJailFilePath,
                    $"Ajaj, <@{eventArgs.Member.Id}>, další dirty coder!"
                );
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to send welcome to the horny jail channel");
            }

            return EventHandlerResult.Continue;
        }
    }
}
