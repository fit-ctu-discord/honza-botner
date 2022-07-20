using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Managers;

public interface IVoiceManager
{
    Task AddNewVoiceChannelAsync(
        DiscordChannel channelToCloneFrom,
        DiscordMember user,
        string? name = null,
        long? limit = null,
        bool? isPublic = null
    );

    Task<bool> EditVoiceChannelAsync(DiscordMember member, string? name, long? limit, bool? isPublic);

    Task DeleteUnusedVoiceChannelAsync(DiscordChannel channel);

    Task DeleteAllUnusedVoiceChannelsAsync();
}
