using System;
using Microsoft.Extensions.DependencyInjection;

namespace HonzaBotner.Discord.EventHandler
{
    public class EventHandlersListBuilder
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly OrderedEventHandlersList _eventHandlersHandlers = new();

        public EventHandlersListBuilder(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public EventHandlersListBuilder AddEventHandler<T>(EventHandlerPriority priority = EventHandlerPriority.Low)
        {
            Type handlerType = typeof(T);
            _eventHandlersHandlers.Add((handlerType, priority));
            _serviceCollection.AddScoped(handlerType);
            return this;
        }

        internal OrderedEventHandlersList Build()
        {
            return _eventHandlersHandlers;
        }
    }
}
