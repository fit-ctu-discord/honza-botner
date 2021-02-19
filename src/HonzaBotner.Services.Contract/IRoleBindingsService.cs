using System.Collections.Generic;
using System.Threading.Tasks;

namespace HonzaBotner.Services.Contract
{
    public interface IRoleBindingsService
    {
        Task<IList<ulong>> FindMappingAsync(ulong channelId, ulong messageId, string? emojiName = null);
        Task AddBindingsAsync(ulong channelId, ulong messageId, string emoji, HashSet<ulong> roleIds);

        /// <summary>
        ///
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="messageId"></param>
        /// <param name="emoji"></param>
        /// <param name="roleIds"></param>
        /// <returns>Some bindings left on message for this emote</returns>
        Task<bool> RemoveBindingsAsync(ulong channelId, ulong messageId, string emoji, HashSet<ulong>? roleIds);
    }
}
