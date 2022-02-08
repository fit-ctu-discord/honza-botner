using System.Threading.Tasks;

namespace HonzaBotner.Discord.EventHandler
{
    /// <summary>
    /// Indicates if the propagation of current event should stop, or if it should be handled by other event handlers.
    /// </summary>
    public enum EventHandlerResult
    {
        /// <summary>
        /// Event should be handled by other event handlers, if there are any available
        /// </summary>
        Continue,
        /// <summary>
        /// Stops propagation of the event to other event handlers.
        /// </summary>
        Stop
    }

    public interface IEventHandler<T>
    {
        Task<EventHandlerResult> Handle(T eventArgs);
    }
}
