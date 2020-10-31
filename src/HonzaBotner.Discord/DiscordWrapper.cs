using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord
{
    public class DiscordWrapper
    {
        private readonly ILogger<DiscordClient> _discordLogger;

        public DiscordClient Client { get; }

        public DiscordWrapper(IOptions<DiscordConfig> options, ILogger<DiscordClient> discordLogger)
        {
            _discordLogger = discordLogger;

            var config = new DiscordConfiguration()
            {
                Token = options.Value.Token, TokenType = TokenType.Bot, Intents = DiscordIntents.All
            };

            discordLogger.LogInformation("Starting with secret: {0}", options.Value.Token);
            Client = new DiscordClient(config);
        }
    }
}
