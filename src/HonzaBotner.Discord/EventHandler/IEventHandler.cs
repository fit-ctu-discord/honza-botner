using System.Threading.Tasks;

namespace HonzaBotner.Discord.EventHandler
{
    public enum EventHandlerResult
    {
        Continue,
        Stop
    }

    public interface IEventHandler<T>
    {
        Task<EventHandlerResult> Handle(T eventArgs);
    }
}
