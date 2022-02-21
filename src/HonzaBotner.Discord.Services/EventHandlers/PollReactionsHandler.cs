using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;

namespace HonzaBotner.Discord.Services.EventHandlers;

public class PollReactionsHandler : IEventHandler<MessageReactionAddEventArgs>
{
    public Task<EventHandlerResult> Handle(MessageReactionAddEventArgs args)
    {
        if (args.User.IsBot) return Task.FromResult(EventHandlerResult.Continue);
        _ = Task.Run(() => HandleAsync(args));
        return Task.FromResult(EventHandlerResult.Continue);
    }

    private async Task HandleAsync(MessageReactionAddEventArgs args)
    {
        DiscordMessage message = await args.Channel.GetMessageAsync(args.Message.Id);
        if (!message.Author.IsCurrent
            || (message.Embeds?.Count.Equals(0) ?? true)
            || !(message.Embeds[0].Footer?.Text.EndsWith("Poll") ?? false))
        {
            return;
        }

        if (message.Reactions.First(x => x.Emoji == args.Emoji).IsMe) return;

        await args.Message.DeleteReactionAsync(args.Emoji, args.User);
    }
}
