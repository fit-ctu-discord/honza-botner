using System;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HonzaBotner.Discord
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDiscordOptions(this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            serviceCollection.Configure<DiscordConfig>(configuration.GetSection(DiscordConfig.ConfigName));
            return serviceCollection;
        }

        public static IServiceCollection AddDiscordBot(this IServiceCollection serviceCollection, Action<CommandsNextExtension> config)
        {
            serviceCollection.AddHostedService<DiscordWorker>();
            serviceCollection.AddSingleton<IDiscordBot, DiscordBot>();
            serviceCollection.AddSingleton<DiscordWrapper>();
            serviceCollection.AddTransient<IGuildProvider, ConfigGuildProvider>();
            serviceCollection.AddSingleton(new CommandConfigurator(config));

            return serviceCollection;
        }
    }
}
