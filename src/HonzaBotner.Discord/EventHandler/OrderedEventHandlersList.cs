using System;
using System.Collections.Generic;

namespace HonzaBotner.Discord.EventHandler
{
    internal sealed class OrderedEventHandlersList : List<Type>
    {
        public OrderedEventHandlersList(IEnumerable<Type> types) : base(types)
        {
        }
    }
}
