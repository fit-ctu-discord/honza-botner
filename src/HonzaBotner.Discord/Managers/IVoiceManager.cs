using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Managers
{
    public interface IVoiceManager
    {
        Task AddNewVoiceChannelAsync(DiscordChannel channelToCloneFrom, DiscordMember user, string? name = null,
            int? limit = 0);

        Task<bool> EditVoiceChannelAsync(DiscordMember user, string? name, int? limit);

        Task DeleteUnusedVoiceChannelAsync(DiscordChannel channel);

        Task DeleteAllUnusedVoiceChannelsAsync();
    }
}
