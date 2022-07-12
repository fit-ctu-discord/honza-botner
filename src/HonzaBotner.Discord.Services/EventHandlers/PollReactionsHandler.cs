using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.EventHandlers;

public class PollReactionsHandler : IEventHandler<MessageReactionAddEventArgs>
{

    private readonly ILogger<PollReactionsHandler> _logger;

    public PollReactionsHandler(ILogger<PollReactionsHandler> logger)
    {
        _logger = logger;
    }

    public Task<EventHandlerResult> Handle(MessageReactionAddEventArgs args)
    {
        if (args.User.IsBot) return Task.FromResult(EventHandlerResult.Continue);
        _ = Task.Run(() => HandleAsync(args));
        return Task.FromResult(EventHandlerResult.Continue);
    }

    private async Task HandleAsync(MessageReactionAddEventArgs args)
    {
        DiscordMessage message;
        try
        {
            message = await args.Channel.GetMessageAsync(args.Message.Id);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e,
                "Failed while fetching message {MessageId} in channel {ChannelId} to check poll reactions",
                args.Message.Id, args.Channel.Id
                );
            return;
        }

        if (!message.Author.IsCurrent
            || (message.Embeds?.Count.Equals(0) ?? true)
            || !(message.Embeds[0].Footer?.Text.EndsWith("Poll") ?? false))
        {
            return;
        }

        if (message.Reactions.FirstOrDefault(x => x.Emoji == args.Emoji)?.IsMe ?? false) return;

        await args.Message.DeleteReactionAsync(args.Emoji, args.User);
    }
}
