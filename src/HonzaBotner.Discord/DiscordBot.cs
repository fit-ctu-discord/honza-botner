using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord
{
    internal class DiscordBot : IDiscordBot
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordWrapper _discordWrapper;
        private readonly ILogger<DiscordBot> _logger;
        private readonly IUrlProvider _urlProvider;
        private readonly CommandConfigurator _configurator;

        private DiscordClient Client => _discordWrapper.Client;
        private CommandsNextExtension Commands => _discordWrapper.Commands;

        public DiscordBot(IServiceProvider provider, DiscordWrapper discordWrapper, ILogger<DiscordBot> logger,
            IUrlProvider urlProvider, CommandConfigurator configurator)
        {
            _provider = provider;
            _discordWrapper = discordWrapper;
            _logger = logger;
            _urlProvider = urlProvider;
            _configurator = configurator;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            Client.Ready += Client_Ready;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.ClientErrored += Client_ClientError;
            Client.MessageReactionAdded += Client_MessageReactionAdded;

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

            if (e.Exception is ChecksFailedException ex)
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

        // TODO: Find out if we need it.
        private async Task Client_MessageReactionAdded(DiscordClient client, MessageReactionAddEventArgs args)
        {
            // TODO: this is only for verify
            var emoji = DiscordEmoji.FromName(Client, ":white_check_mark:");

            // https://discordapp.com/channels/366970031445377024/507515506073403402/686745124885364770
            if (!(args.Message.Id == 686745124885364770 && args.Message.ChannelId == 507515506073403402)) return;
            if (!args.Emoji.Equals(emoji)) return;

            _logger.Log(LogLevel.Information, "THIS IS THE RIGHT MESSAGE FOR VERIFY");


            DiscordUser user = args.User;
            DiscordDmChannel channel = await args.Guild.Members[user.Id].CreateDmChannelAsync();

            using var scope = _provider.CreateScope();
            var authorizationService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();

            if (await authorizationService.IsUserVerified(user.Id))
            {
                await channel.SendMessageAsync("You are already authorized");
            }
            else
            {
                string link = _urlProvider.GetAuthLink(user.Id);
                await channel.SendMessageAsync($"Hi, authorize by following this link: {link}");
            }
        }
    }
}
