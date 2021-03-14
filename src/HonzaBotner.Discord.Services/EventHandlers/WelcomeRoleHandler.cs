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
    public class WelcomeRoleHandler : IEventHandler<GuildMemberUpdateEventArgs>
    {
        private readonly ILogger<WelcomeRoleHandler> _logger;
        private readonly CommonCommandOptions _commonOptions;

        public WelcomeRoleHandler(ILogger<WelcomeRoleHandler> logger, IOptions<CommonCommandOptions> commonOptions)
        {
            _logger = logger;
            _commonOptions = commonOptions.Value;
        }

        public async Task<EventHandlerResult> Handle(GuildMemberUpdateEventArgs eventArgs)
        {
            List<Func<GuildMemberUpdateEventArgs, Task<EventHandlerResult>>> functions = new()
            {
                HandleAddedHornyJailRole, HandleAddedBoosterRole
            };

            foreach (var function in functions)
            {
                if (await function(eventArgs) == EventHandlerResult.Stop)
                {
                    return EventHandlerResult.Stop;
                }
            }

            return EventHandlerResult.Continue;
        }

        private async Task<EventHandlerResult> HandleAddedHornyJailRole(GuildMemberUpdateEventArgs eventArgs)
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

        private async Task<EventHandlerResult> HandleAddedBoosterRole(GuildMemberUpdateEventArgs eventArgs)
        {
            try
            {
                DiscordRole boosterRole = eventArgs.Guild.GetRole(_commonOptions.BoosterRoleId);
                if (eventArgs.RolesBefore.Contains(boosterRole) || !eventArgs.RolesAfter.Contains(boosterRole))
                {
                    return EventHandlerResult.Continue;
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Couldn't get booster role");
            }

            try
            {
                DiscordChannel channel = eventArgs.Guild.GetChannel(_commonOptions.GentlemenChannelId);
                await channel.SendFileAsync(
                    _commonOptions.GentlemenFilePath,
                    $"Vítej v první třídě, <@{eventArgs.Member.Id}>!"
                );
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to send welcome to the gentlemen channel");
            }

            return EventHandlerResult.Continue;
        }
    }
}
