using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.EventHandlers.Helpers
{
    public abstract class WelcomeMessageHandler
    {
        public async Task<EventHandlerResult> HandleAddedRole(
            GuildMemberUpdateEventArgs eventArgs,
            ulong roleId,
            ulong channelId,
            string filePath,
            string message,
            ILogger? logger = null
        )
        {
            try
            {
                DiscordRole hornyJailRole = eventArgs.Guild.GetRole(roleId);
                if (eventArgs.RolesBefore.Contains(hornyJailRole) || !eventArgs.RolesAfter.Contains(hornyJailRole))
                {
                    return EventHandlerResult.Continue;
                }
            }
            catch (Exception e)
            {
                logger?.LogWarning(e, "Couldn't get the role");
            }

            try
            {
                DiscordChannel channel = eventArgs.Guild.GetChannel(channelId);
                await channel.SendFileAsync(
                    filePath,
                    message
                );
            }
            catch (Exception e)
            {
                logger?.LogError(e, "Failed to send welcome message to the channel");
            }

            return EventHandlerResult.Continue;
        }
    }
}
