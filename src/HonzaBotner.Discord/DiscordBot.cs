using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using HonzaBotner.Discord.Command;
using HonzaBotner.Services.Contract;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace HonzaBotner.Discord
{
    internal class DiscordBot : IDiscordBot
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordWrapper _discordWrapper;
        private readonly ILogger<DiscordBot> _logger;
        private readonly CommandCollection _commands;
        private readonly IUrlProvider _urlProvider;

        private DiscordClient Client => _discordWrapper.Client;

        public DiscordBot(IServiceProvider provider, DiscordWrapper discordWrapper, ILogger<DiscordBot> logger,
            CommandCollection commands, IUrlProvider urlProvider)
        {
            _provider = provider;
            _discordWrapper = discordWrapper;
            _logger = logger;
            _commands = commands;
            _urlProvider = urlProvider;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            await Client.ConnectAsync();

            Client.MessageCreated += (client, args) => OnClientOnMessageCreated(args, cancellationToken);

            Client.MessageReactionAdded += OnClientOnMessageReactionAdded;

            await Task.Delay(-1, cancellationToken);
        }

        private bool GetCommand(MessageCreateEventArgs args, out IChatCommand commandProvider,
            IServiceScope? scope = null)
        {
            var provider = scope == null ? _provider : scope.ServiceProvider;

            string message = args.Message.Content.Substring(1).Split(" ")[0]!;
            _logger.LogInformation(message);
            var command = _commands.GetCommandType(message);
            _logger.LogInformation(command?.ToString());

            if (command == null)
            {
                _logger.LogWarning("Couldn't find command for message '{0}'", args.Message.Content);
                commandProvider = null!;
                return false;
            }

            var service = provider.GetService(command);
            if (service is IChatCommand c)
            {
                commandProvider = c;
                return true;
            }

            // Shouldn't happen at all
            _logger.LogError("Couldn't find {0} in DI context", command.Name);
            commandProvider = null!;
            return false;
        }

        private async Task OnClientOnMessageCreated(MessageCreateEventArgs args, CancellationToken cancellationToken)
        {
            if (!args.Message.Content.StartsWith(";")) return;

            using var scope = _provider.CreateScope();

            if (!GetCommand(args, out var commandProvider, scope)) return;

            try
            {
                var commandResult = await commandProvider.ExecuteAsync(Client, args.Message, cancellationToken);

                switch (commandResult)
                {
                    case ChatCommendExecutedResult.Ok:
                        await args.Message.CreateReactionAsync(DiscordEmoji.FromName(Client, ":white_check_mark:"));
                        break;
                    case ChatCommendExecutedResult.InternalError:
                        await args.Message.CreateReactionAsync(DiscordEmoji.FromName(Client, ":x:"));
                        break;
                    case ChatCommendExecutedResult.CannotBeUsedByBot:
                        await args.Message.CreateReactionAsync(DiscordEmoji.FromName(Client, ":robot:"));
                        break;
                    default:
                        await args.Message.CreateReactionAsync(DiscordEmoji.FromName(Client, ":warning:"));
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e!.ToString());
                await args.Message.CreateReactionAsync(DiscordEmoji.FromName(Client, ":no_entry:"));
            }
        }

        private async Task OnClientOnMessageReactionAdded(DiscordClient client, MessageReactionAddEventArgs args)
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
                await channel.SendMessageAsync($"You are already authorized");
            }
            else
            {
                string link = _urlProvider.GetAuthLink(user.Id);
                await channel.SendMessageAsync($"Hi, authorize by following this link: {link}");
            }
        }
    }
}
