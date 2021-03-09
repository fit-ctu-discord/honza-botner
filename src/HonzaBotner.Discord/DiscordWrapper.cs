using System;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord
{
    public class DiscordWrapper
    {
        public DiscordClient Client { get; }
        public CommandsNextExtension Commands { get; }
        public InteractivityExtension Interactivity { get; }

        public DiscordWrapper(IOptions<DiscordConfig> options, IServiceProvider services, ILoggerFactory loggerFactory)
        {
            DiscordConfig optionsConfig = options.Value;
            DiscordConfiguration config = new()
            {
                LoggerFactory = loggerFactory,
                Token = optionsConfig.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All
            };

            Client = new DiscordClient(config);

            CommandsNextConfiguration cConfig = new()
            {
                Services = services, StringPrefixes = optionsConfig.Prefixes, EnableDms = true
            };
            Commands = Client.UseCommandsNext(cConfig);

            InteractivityConfiguration iConfig = new()
            {
                PollBehaviour = PollBehaviour.KeepEmojis, Timeout = TimeSpan.FromSeconds(30)
            };
            Interactivity = Client.UseInteractivity(iConfig);

            Client.Logger.LogInformation("Starting with secret: {Token}", options.Value.Token);
        }
    }
}
