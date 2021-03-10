using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Services.Contract;

namespace HonzaBotner.Discord.Services.EventHandlers
{
    public class EmojiCounterHandler : IEventHandler<MessageReactionAddEventArgs>,
        IEventHandler<MessageReactionRemoveEventArgs>
    {
        private readonly IEmojiCounterService _emojiCounterService;

        public EmojiCounterHandler(IEmojiCounterService emojiCounterService)
        {
            _emojiCounterService = emojiCounterService;
        }

        public async Task<EventHandlerResult> Handle(MessageReactionAddEventArgs eventArgs)
        {
            DiscordEmoji emoji = eventArgs.Emoji;

            if (eventArgs.Guild.Emojis.ContainsKey(emoji.Id))
            {
                await _emojiCounterService.IncrementAsync(emoji.Id);
            }

            return EventHandlerResult.Continue;
        }

        public async Task<EventHandlerResult> Handle(MessageReactionRemoveEventArgs eventArgs)
        {
            DiscordEmoji emoji = eventArgs.Emoji;

            if (eventArgs.Guild.Emojis.ContainsKey(emoji.Id))
            {
                await _emojiCounterService.DecrementAsync(emoji.Id);
            }

            return EventHandlerResult.Continue;
        }
    }
}
