using System.Collections.Generic;
using System.Threading.Tasks;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Services.Contract
{
    public interface IEmojiCounterService
    {
        Task<IEnumerable<CountedEmoji>> ListAsync();
        Task IncrementAsync(ulong emojiId);
        Task DecrementAsync(ulong emojiId);
    }
}
