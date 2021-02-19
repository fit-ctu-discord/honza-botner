using System;
using Microsoft.Extensions.DependencyInjection;

namespace HonzaBotner.Discord
{
    public class ReactionListBuilder
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly OrderedReactionHandlersList _reactionHandlersHandlers = new();

        public ReactionListBuilder(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public ReactionListBuilder AddReaction<T>() where T : IReactionHandler
        {
            Type handlerType = typeof(T);
            _reactionHandlersHandlers.Add(handlerType);
            _serviceCollection.AddScoped(handlerType);
            return this;
        }

        internal OrderedReactionHandlersList Build()
        {
            return _reactionHandlersHandlers;
        }
    }
}
