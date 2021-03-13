using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace HonzaBotner.Discord.EventHandler
{
    internal class EventHandler
    {
        private readonly IServiceProvider _provider;
        private readonly OrderedEventHandlersList _eventHandlers;

        public EventHandler(IServiceProvider provider, OrderedEventHandlersList eventHandlers)
        {
            _provider = provider;
            _eventHandlers = eventHandlers;
        }

        public async Task Handle<T>(T eventArgs) where T : EventArgs
        {
            using IServiceScope scope = _provider.CreateScope();

            // Sort handlers in descending order.
            _eventHandlers.Sort((first, second) => second.Item2.CompareTo(first.Item2));

            foreach ((Type reactionHandlerType, _) in _eventHandlers)
            {
                Console.WriteLine(reactionHandlerType);
                if (!reactionHandlerType.IsAssignableTo(typeof(IEventHandler<T>)))
                {
                    continue;
                }

                IEventHandler<T> handler =
                    scope.ServiceProvider.GetService(reactionHandlerType) as IEventHandler<T>
                    ?? throw new ArgumentOutOfRangeException();
                EventHandlerResult shouldStop =
                    await handler.Handle(eventArgs).ConfigureAwait(false);
                if (shouldStop == EventHandlerResult.Stop) return;
            }
        }
    }
}
