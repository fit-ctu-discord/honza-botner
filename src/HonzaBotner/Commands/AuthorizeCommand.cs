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
    public class AuthorizeCommand : IChatCommand
    {
        private const string LinkTemplate = "https://localhost:5001/Auth?gid={0}&uid={1}";

        public const string CommandText = "authorize";

        public async Task ExecuteAsync(DiscordClient client, DiscordMessage message, CancellationToken cancellationToken = default)
        {
            DiscordUser user = message.Author;

            DiscordDmChannel channel = await client.CreateDmAsync(user);

            string link = string.Format(LinkTemplate, message.Channel.GuildId, user.Id);

            await channel.SendMessageAsync($"Hi, authorize by following this link: {link}");
        }
    }
}
