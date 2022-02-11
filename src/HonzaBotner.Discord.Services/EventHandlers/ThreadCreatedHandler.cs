using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;

namespace HonzaBotner.Discord.Services.EventHandlers;

public class ThreadCreatedHandler : IEventHandler<ThreadCreateEventArgs>
{
    public async Task<EventHandlerResult> Handle(ThreadCreateEventArgs args)
    {
        await args.Thread.JoinThreadAsync();
        return EventHandlerResult.Continue;
    }

}
