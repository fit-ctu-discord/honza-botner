using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace OsBot.Core.Command
{
    public interface IChatCommand
    {
        Task Execute(DiscordClient client, DiscordMessage message, CancellationToken cancellationToken);
    }
}