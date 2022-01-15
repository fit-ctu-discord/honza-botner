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
        DiscordMessage message = await args.Channel.GetMessageAsync(args.Message.Id);
        if (!message.Author.IsCurrent
            || (message.Embeds?.Count.Equals(0) ?? true)
            || !(message.Embeds[0].Footer?.Text.EndsWith("Poll") ?? false))
        {
            return EventHandlerResult.Continue;
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
                return EventHandlerResult.Continue;
        }
        foreach (var emoji in poll.ActiveEmojis)
        {
            if(DiscordEmoji.FromName(_client, emoji) == args.Emoji) return EventHandlerResult.Continue;
        }

        await args.Message.DeleteReactionAsync(args.Emoji, args.User);
        return EventHandlerResult.Stop;
    }
}
