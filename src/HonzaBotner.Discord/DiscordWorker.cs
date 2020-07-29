using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace OsBot.Core
{
    public class DiscordWorker : BackgroundService
    {
        private readonly IDiscordBot _discordBot;

        public DiscordWorker(IDiscordBot discordBot)
        {
            _discordBot = discordBot;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _discordBot.Run(stoppingToken);
        }
    }
}