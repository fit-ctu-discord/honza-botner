using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Managers
{
    public interface IVoiceManager
    {
        Task AddNewVoiceChannelAsync(DiscordChannel channelToCloneFrom, DiscordMember user,
            string? name = null, int? limit = null, bool? isPublic = null);

        Task<bool> EditVoiceChannelAsync(DiscordMember user, string? name, int? limit, bool? isPublic);

        Task DeleteUnusedVoiceChannelAsync(DiscordChannel channel);

        Task DeleteAllUnusedVoiceChannelsAsync();
    }
}
