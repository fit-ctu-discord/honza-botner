using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Command;

namespace HonzaBotner.Commands
{
    public class Activity : IChatCommand
    {
        public const string ChatCommand = "activity";
        // ;activity {play, watch, listen} <name>

        public async Task ExecuteAsync(DiscordClient client, DiscordMessage message, CancellationToken cancellationToken)
        {
            if (message.Author.IsBot) return;
            if (message.Content.Split(" ").Length < 3) return;

            string type = message.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1];
            string name = message.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries)[2];

            await client.UpdateStatusAsync(new DiscordGame(name));
        }
    }
}
