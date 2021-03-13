using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace HonzaBotner.Discord.EventHandler
{
    public class EventHandlersListBuilder
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly List<(Type, EventHandlerPriority)> _eventHandlersHandlers = new();

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
            return new(_eventHandlersHandlers
                .OrderBy(x => x.Item2)
                .Select(tuple => tuple.Item1)
            );
        }
    }
}
