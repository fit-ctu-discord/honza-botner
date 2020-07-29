using System.Threading;
using System.Threading.Tasks;

namespace OsBot.Core
{
    public interface IDiscordBot
    {
        Task Run(CancellationToken cancellationToken);
    }
}