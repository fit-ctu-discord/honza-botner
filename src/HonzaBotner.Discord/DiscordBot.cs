using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OsBot.Core.Command;

namespace OsBot.Core
{
    internal class DiscordBot : IDiscordBot
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordWrapper _discordWrapper;
        private readonly ILogger<DiscordBot> _logger;
        private readonly CommandCollection _commands;

        private DiscordClient Client => _discordWrapper.Client;

        public DiscordBot(IServiceProvider provider, DiscordWrapper discordWrapper, ILogger<DiscordBot> logger,
            CommandCollection commands)
        {
            _provider = provider;
            _discordWrapper = discordWrapper;
            _logger = logger;
            _commands = commands;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            await Client.ConnectAsync();

            Client.MessageCreated += (args) => OnClientOnMessageCreated(args, cancellationToken);

            await Task.Delay(-1, cancellationToken);
        }

        private bool GetCommand(MessageCreateEventArgs args, out IChatCommand commandProvider,
            IServiceScope? scope = null)
        {
            var provider = scope == null ? _provider : scope.ServiceProvider;

            var message = args.Message.Content.Substring(1);
            var command = _commands.GetCommandType(message);

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

        private Task OnClientOnMessageCreated(MessageCreateEventArgs args, CancellationToken cancellationToken)
        {
            if (!args.Message.Content.StartsWith("!")) return Task.CompletedTask;

            using var scope = _provider.CreateScope();
            
            if (!GetCommand(args, out var commandProvider, scope)) return Task.CompletedTask;

            var _ = commandProvider.Execute(Client, args.Message, cancellationToken)
                .ContinueWith(t => _logger.LogError(t.Exception!.ToString()),
                    TaskContinuationOptions.OnlyOnFaulted);

            return Task.CompletedTask;
        }
    }
}