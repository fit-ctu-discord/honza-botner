using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.EventHandlers.Helpers;

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
            DiscordRole role = eventArgs.Guild.GetRole(roleId);
            if (eventArgs.RolesBefore.Contains(role) || !eventArgs.RolesAfter.Contains(role))
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
            await channel.SendMessageAsync(
                new DiscordMessageBuilder()
                    .WithContent(message)
                    .WithFile(new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    .WithAllowedMention(new UserMention(eventArgs.Member))
            );
        }
        catch (Exception e)
        {
            logger?.LogError(e, "Failed to send welcome message to the channel");
        }

        return EventHandlerResult.Continue;
    }
}
