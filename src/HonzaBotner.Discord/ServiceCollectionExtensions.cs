using System;
using DSharpPlus.CommandsNext;
using HonzaBotner.Discord.EventHandler;
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

        public static IServiceCollection AddDiscordBot(this IServiceCollection serviceCollection,
            Action<CommandsNextExtension> commandConfig, Action<EventHandlersListBuilder> reactionConfig)
        {
            serviceCollection.AddHostedService<DiscordWorker>();
            serviceCollection.AddSingleton<IDiscordBot, DiscordBot>();
            serviceCollection.AddSingleton<DiscordWrapper>();
            serviceCollection.AddTransient<IGuildProvider, ConfigGuildProvider>();
            serviceCollection.AddTransient<EventHandler.EventHandler>();

            serviceCollection.AddSingleton(new CommandConfigurator(commandConfig));

            EventHandlersListBuilder builder = new(serviceCollection);
            reactionConfig(builder);
            serviceCollection.AddSingleton(builder.Build());

            return serviceCollection;
        }
    }
}
