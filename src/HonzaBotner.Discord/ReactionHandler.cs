using System;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;

namespace HonzaBotner.Discord
{
    internal class ReactionHandler
    {
        private readonly IServiceProvider _provider;
        private readonly OrderedReactionHandlersList _reactionsHandlers;

        public ReactionHandler(IServiceProvider provider, OrderedReactionHandlersList reactionsHandlers)
        {
            _provider = provider;
            _reactionsHandlers = reactionsHandlers;
        }

        public async Task HandleAddAsync(MessageReactionAddEventArgs eventArgs)
        {
            using IServiceScope scope = _provider.CreateScope();

            foreach (Type reactionHandlerType in _reactionsHandlers)
            {
                IReactionHandler handler = scope.ServiceProvider.GetService(reactionHandlerType) as IReactionHandler
                                           ?? throw new ArgumentOutOfRangeException();
                var shouldStop = await handler.HandleAddAsync(eventArgs).ConfigureAwait(false);
                if (shouldStop == IReactionHandler.Result.Stop) return;
            }
        }

        public async Task HandleRemoveAsync(MessageReactionRemoveEventArgs eventArgs)
        {
            using IServiceScope scope = _provider.CreateScope();

            foreach (Type reactionHandlerType in _reactionsHandlers)
            {
                IReactionHandler handler = scope.ServiceProvider.GetService(reactionHandlerType) as IReactionHandler
                                           ?? throw new ArgumentOutOfRangeException();
                var shouldStop = await handler.HandleRemoveAsync(eventArgs).ConfigureAwait(false);
                if (shouldStop == IReactionHandler.Result.Stop) return;
            }
        }
    }
}
