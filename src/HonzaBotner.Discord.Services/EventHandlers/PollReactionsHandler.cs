using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Services.Commands.Polls;

namespace HonzaBotner.Discord.Services.EventHandlers;

public class PollReactionsHandler : IEventHandler<MessageReactionAddEventArgs>
{
    private readonly DiscordClient _client;

    public PollReactionsHandler(DiscordWrapper wrapper)
    {
        _client = wrapper.Client;
    }
    public async Task<EventHandlerResult> Handle(MessageReactionAddEventArgs args)
    {
        if (args.User.IsBot) return EventHandlerResult.Continue;
        _ = Task.Run(() => HandleAsync(args));
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

        Poll poll;
        switch (message.Embeds[0].Footer.Text)
        {
            case "AbcPoll":
                poll = new AbcPoll(message);
                break;
            case "YesNoPoll":
                poll = new YesNoPoll(message);
                break;
            default:
                return;
        }
        if (poll.ActiveEmojis.Any(emoji => DiscordEmoji.FromName(_client, emoji) == args.Emoji))
        {
            return;
        }

        await args.Message.DeleteReactionAsync(args.Emoji, args.User);
    }
}
