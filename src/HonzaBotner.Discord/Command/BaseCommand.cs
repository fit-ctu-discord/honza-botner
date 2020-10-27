using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Command
{
    public abstract class BaseCommand : IChatCommand
    {
        private readonly IPermissionHandler _permissionHandler;
        protected readonly ILogger _logger;

        protected BaseCommand(IPermissionHandler permissionHandler, ILogger logger)
        {
            _permissionHandler = permissionHandler;
            _logger = logger;
        }

        protected virtual CommandPermission RequiredPermission => CommandPermission.Authorized;

        protected virtual bool CanBotExecute => true;

        protected virtual Task<bool> CanExecuteAsync(DiscordUser user)
        {
            return _permissionHandler.HasRightsAsync(user.Id, RequiredPermission);
        }

        protected abstract Task<ChatCommendExecutedResult> ExecuteAsync(DiscordClient client, DiscordMessage message,
            CancellationToken cancellationToken = default);

        async Task<ChatCommendExecutedResult> IChatCommand.ExecuteAsync(DiscordClient client, DiscordMessage message,
            CancellationToken cancellationToken)
        {
            if (!await CanExecuteAsync(message.Author))
            {
                return ChatCommendExecutedResult.InsufficientPermission;
            }
            else if (!CanBotExecute && message.Author.IsBot)
            {
                return ChatCommendExecutedResult.CannotBeUsedByBot;
            }

            try
            {
                return await ExecuteAsync(client, message, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return ChatCommendExecutedResult.InternalError;
            }
        }
    }
}
