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
                Token = options.Value.Token,
                TokenType = TokenType.Bot
            };

            Client = new DiscordClient(config);
            Client.DebugLogger.LogMessageReceived += HandleLog;
        }

        private void HandleLog(object? sender, DebugLogMessageEventArgs args)
        {
            var level = args.Level.ToLoggingLevel();
            _discordLogger.Log(level, $"[{args.Application}]: {args.Message}");
        }
    }
}
