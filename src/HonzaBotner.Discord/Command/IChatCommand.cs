using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Command
{
    public interface IChatCommand
    {
        IEnumerable<string> RequiredPermissions { get; }

        Task ExecuteAsync(DiscordClient client, DiscordMessage message, CancellationToken cancellationToken);
    }
}
