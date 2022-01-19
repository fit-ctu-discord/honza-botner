using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;

namespace HonzaBotner.Discord.Services.EventHandlers;

public class PollReactionsHandler : IEventHandler<MessageReactionAddEventArgs>
{
    public async Task<EventHandlerResult> Handle(MessageReactionAddEventArgs args)
    {
        if (args.User.IsBot) return EventHandlerResult.Continue;
        _ = Task.Run(() => HandleAsync(args));
        await Task.Delay(0);
        return EventHandlerResult.Continue;
    }

    private async Task HandleAsync(MessageReactionAddEventArgs args)
    {
        DiscordMessage message = args.Message.Content is null
            ? await args.Channel.GetMessageAsync(args.Message.Id)
            : args.Message;
        if (!message.Author.IsCurrent
            || (message.Embeds?.Count.Equals(0) ?? true)
            || !(message.Embeds[0].Footer?.Text.EndsWith("Poll") ?? false))
        {
            return;
        }

        if (message.Reactions.Any(x => x.Emoji == args.Emoji && x.Count > 1)) return;

        await args.Message.DeleteReactionAsync(args.Emoji, args.User);
    }
}
