using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers;

public class EmojiCounterHandler : IEventHandler<MessageReactionAddEventArgs>,
    IEventHandler<MessageReactionRemoveEventArgs>
{
    private readonly IEmojiCounterService _emojiCounterService;
    private readonly ulong[] _ignoreChannels;

    public EmojiCounterHandler(IEmojiCounterService emojiCounterService, IOptions<CommonCommandOptions> options)
    {
        _emojiCounterService = emojiCounterService;
        _ignoreChannels = options.Value.ReactionIgnoreChannels ?? Array.Empty<ulong>();
    }

    public async Task<EventHandlerResult> Handle(MessageReactionAddEventArgs eventArgs)
    {
        DiscordEmoji emoji = eventArgs.Emoji;

        if (eventArgs.Guild.Emojis.ContainsKey(emoji.Id) && !_ignoreChannels.Contains(eventArgs.Channel.Id))
        {
            await _emojiCounterService.IncrementAsync(emoji.Id);
        }

        return EventHandlerResult.Continue;
    }

    public async Task<EventHandlerResult> Handle(MessageReactionRemoveEventArgs eventArgs)
    {
        DiscordEmoji emoji = eventArgs.Emoji;

        if (eventArgs.Guild.Emojis.ContainsKey(emoji.Id) && !_ignoreChannels.Contains(eventArgs.Channel.Id))
        {
            await _emojiCounterService.DecrementAsync(emoji.Id);
        }

        return EventHandlerResult.Continue;
    }
}
