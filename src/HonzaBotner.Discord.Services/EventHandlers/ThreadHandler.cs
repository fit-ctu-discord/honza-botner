using System;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.EventHandlers;

public class ThreadHandler : IEventHandler<ThreadCreateEventArgs>
{
    private readonly ILogger<ThreadHandler> _logger;

    public ThreadHandler(ILogger<ThreadHandler> logger)
    {
        _logger = logger;
    }

    public async Task<EventHandlerResult> Handle(ThreadCreateEventArgs args)
    {
        try
        {
            await args.Thread.JoinThreadAsync();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to join {ThreadName} created in channel {ChannelName}",
                args.Thread.Name, args.Parent.Name);
        }

        return EventHandlerResult.Continue;
    }
}
