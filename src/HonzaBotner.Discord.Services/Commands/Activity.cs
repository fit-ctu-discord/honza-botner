using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Command;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Commands
{
    public class Activity : BaseCommand
    {
        public const string ChatCommand = "activity";
        // ;activity {play, watch, listen} <name>

        public Activity(IPermissionHandler permissionHandler, ILogger<Activity> logger)
            : base(permissionHandler, logger)
        {
        }

        protected override async Task<ChatCommendExecutedResult> ExecuteAsync(DiscordClient client,
            DiscordMessage message, CancellationToken cancellationToken = default)
        {
            if (message.Author.IsBot) return ChatCommendExecutedResult.CannotBeUsedByBot;
            if (message.Content.Split(" ").Length < 3) return ChatCommendExecutedResult.WrongSyntax;

            string _ = message.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1];
            string name = message.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries)[2];

            await client.UpdateStatusAsync(new DiscordActivity(name, ActivityType.Custom));

            return ChatCommendExecutedResult.Ok;
        }
    }
}
