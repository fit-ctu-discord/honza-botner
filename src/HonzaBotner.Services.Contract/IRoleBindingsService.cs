using System.Collections.Generic;
using System.Threading.Tasks;

namespace HonzaBotner.Services.Contract
{
    public interface IRoleBindingsService
    {
        Task<IList<ulong>> FindMappingAsync(ulong channelId, ulong messageId, string emojiName);
        Task AddBindingsAsync(ulong channelId, ulong messageId, string emoji, HashSet<ulong> roleIds);
        Task RemoveBindingsAsync(ulong channelId, ulong messageId, string emoji, HashSet<ulong>? roleIds);
    }
}
