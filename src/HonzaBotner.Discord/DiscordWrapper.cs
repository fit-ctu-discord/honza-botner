using System;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord
{
    public class DiscordWrapper
    {
        public DiscordClient Client { get; }
        public CommandsNextExtension Commands { get; }

        public DiscordWrapper(IOptions<DiscordConfig> options, IServiceProvider services, CommandConfigurator configurator, ILoggerFactory loggerFactory)
        {
            DiscordConfig optionsConfig = options.Value;
            var config = new DiscordConfiguration()
            {
                LoggerFactory = loggerFactory,
                Token = optionsConfig.Token, TokenType = TokenType.Bot, Intents = DiscordIntents.All
            };


            Client = new DiscordClient(config);

            CommandsNextConfiguration cConfig = new()
            {
                Services = services, StringPrefixes = optionsConfig.Prefixes, EnableDms = true
            };

            Commands = Client.UseCommandsNext(cConfig);

            Client.Logger.LogInformation("Starting with secret: {0}", options.Value.Token);
        }

    }
}
