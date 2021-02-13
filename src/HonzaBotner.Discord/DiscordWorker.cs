using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace HonzaBotner.Discord
{
    public class DiscordWorker : BackgroundService
    {
        private readonly IDiscordBot _discordBot;
        private readonly IVoiceManager _voiceManager;

        public DiscordWorker(IDiscordBot discordBot, IVoiceManager voiceManager)
        {
            _discordBot = discordBot;
            _voiceManager = voiceManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // TODO
            _discordBot.Run(stoppingToken);
            _voiceManager.Run(stoppingToken);
        }
    }
}
