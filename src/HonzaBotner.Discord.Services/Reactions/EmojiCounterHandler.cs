using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Services.Contract;

namespace HonzaBotner.Discord.Services.Reactions
{
    public class EmojiCounterHandler : IReactionHandler
    {
        private readonly IEmojiCounterService _emojiCounterService;

        public EmojiCounterHandler(IEmojiCounterService emojiCounterService)
        {
            _emojiCounterService = emojiCounterService;
        }

        public async Task<IReactionHandler.Result> HandleAddAsync(MessageReactionAddEventArgs eventArgs)
        {
            DiscordEmoji emoji = eventArgs.Emoji;

            if (eventArgs.Guild.Emojis.ContainsKey(emoji.Id))
            {
                await _emojiCounterService.IncrementAsync(emoji.Id);
            }

            return IReactionHandler.Result.Continue;
        }

        public async Task<IReactionHandler.Result> HandleRemoveAsync(MessageReactionRemoveEventArgs eventArgs)
        {
            DiscordEmoji emoji = eventArgs.Emoji;

            if (eventArgs.Guild.Emojis.ContainsKey(emoji.Id))
            {
                await _emojiCounterService.DecrementAsync(emoji.Id);
            }

            return IReactionHandler.Result.Continue;
        }
    }
}
