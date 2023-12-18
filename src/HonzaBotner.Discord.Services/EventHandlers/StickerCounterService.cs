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

public class StickerCounterService : IEventHandler<MessageCreateEventArgs>
{
    private readonly IEmojiCounterService _emojiCounterService;
    private readonly ulong[] _ignoreChannels;

    public StickerCounterService(IEmojiCounterService emojiCounterService, IOptions<CommonCommandOptions> options)
    {
        _emojiCounterService = emojiCounterService;
        _ignoreChannels = options.Value.ReactionIgnoreChannels ?? Array.Empty<ulong>();
    }

    public async Task<EventHandlerResult> Handle(MessageCreateEventArgs args)
    {
        if (args.Message.Stickers.Count == 0 || _ignoreChannels.Contains(args.Channel.Id))
        {
            return EventHandlerResult.Continue;
        }

        DiscordMessageSticker sticker = args.Message.Stickers[0];

        if (sticker.Type == StickerType.Guild && args.Guild.Stickers.Keys.Contains(sticker.Id))
        {
            await _emojiCounterService.IncrementAsync(sticker.Id);
        }

        return EventHandlerResult.Continue;
    }
}
