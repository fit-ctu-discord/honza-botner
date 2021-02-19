using System.Collections.Generic;
using System.Threading.Tasks;

namespace HonzaBotner.Services.Contract
{
    public interface IZiraService
    {
        Task<IList<ulong>> FindMappingAsync(ulong channelId, ulong messageId, string emojiName);
    }
}
