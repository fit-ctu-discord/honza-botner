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
    public class HiCommand : IChatCommand
    {
        public const string ChatCommand = "hi";

        public async Task<ChatCommendExecutedResult> ExecuteAsync(DiscordClient client, DiscordMessage message, CancellationToken cancellationToken)
        {
            await client.SendMessageAsync(message.Channel, "Hi");

            return ChatCommendExecutedResult.Ok;
        }
    }
}
