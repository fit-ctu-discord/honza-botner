using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord
{
    internal class DiscordBot : IDiscordBot
    {
        private readonly DiscordWrapper _discordWrapper;
        private readonly ReactionHandler _reactionHandler;
        private readonly CommandConfigurator _configurator;

        private DiscordClient Client => _discordWrapper.Client;
        private CommandsNextExtension Commands => _discordWrapper.Commands;

        public DiscordBot(DiscordWrapper discordWrapper, ReactionHandler reactionHandler, CommandConfigurator configurator)
        {
            _discordWrapper = discordWrapper;
            _reactionHandler = reactionHandler;
            _configurator = configurator;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            Client.Ready += Client_Ready;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.ClientErrored += Client_ClientError;
            Client.MessageReactionAdded += Client_MessageReactionAdded;
            Client.MessageReactionRemoved += Client_MessageReactionRemoved;

            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;

            _configurator.Config(Commands);

            await Client.ConnectAsync();
            await Task.Delay(-1, cancellationToken);
        }

        private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            sender.Logger.LogInformation("Client is ready to process events");
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            sender.Logger.LogInformation($"Guild available: {e.Guild.Name}");
            return Task.CompletedTask;
        }

        private Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
        {
            sender.Logger.LogError(e.Exception, "Exception occured");
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(
                $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            e.Context.Client.Logger.LogError(
                $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}",
                DateTime.Now);

            if (e.Exception is ChecksFailedException)
            {
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Přístup zakázán",
                    Description = $"{emoji} Na vykonání příkazu nemáte dostatečná práva. Pokud si myslíte že ano, kontaktujte svého MODa.",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await e.Context.RespondAsync("", embed: embed);
            }
        }

        private Task Client_MessageReactionAdded(DiscordClient client, MessageReactionAddEventArgs args)
        {
            return _reactionHandler.HandleAddAsync(args);
        }

        private Task Client_MessageReactionRemoved(DiscordClient client, MessageReactionRemoveEventArgs args)
        {
            return _reactionHandler.HandleRemoveAsync(args);
        }
    }
}
