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
    public class SendMessageCommand : IChatCommand
    {
        public const string ChatCommand = "send";

        public async Task ExecuteAsync(DiscordClient client, DiscordMessage message,
            CancellationToken cancellationToken)
        {
            if (message.MentionedChannels.Count.Equals(0)) return;
            var channel = message.MentionedChannels[0];
            string sendMessage = string.Join(" ",
                message.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries).Skip(1));
            await client.SendMessageAsync(channel, sendMessage);
        }
    }
}
