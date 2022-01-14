using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;

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
        if (!message.Author.IsCurrent) return EventHandlerResult.Continue;
        if (message.Embeds?.Count.Equals(0) ?? true) return EventHandlerResult.Continue;
        if (!(message.Embeds[0].Footer?.Text.Equals("AbcPoll") ?? false)) return EventHandlerResult.Continue;

        DiscordEmbed poll = message.Embeds[0];
        int questions = poll.Fields.Count;
        for (char i = 'a'; i < questions + 'a'; i++)
        {
            DiscordEmoji testingEmoji = DiscordEmoji.FromName(_client, ":regional_indicator_" + i + ":");
            if(testingEmoji == args.Emoji) return EventHandlerResult.Continue;
        }
        await args.Message.DeleteReactionAsync(args.Emoji, args.User);
        return EventHandlerResult.Stop;

    }
}
