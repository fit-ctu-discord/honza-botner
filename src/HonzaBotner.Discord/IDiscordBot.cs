using System.Threading;
using System.Threading.Tasks;

namespace HonzaBotner.Discord;

public interface IDiscordBot
{
    Task Run(CancellationToken cancellationToken);
}
