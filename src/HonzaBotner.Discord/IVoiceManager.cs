using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord
{
    public interface IVoiceManager
    {
        Task Init();

        Task AddNewVoiceChannelAsync(DiscordChannel channelToCloneFrom, DiscordMember user, string? name, int? limit);

        Task<bool> EditVoiceChannelAsync(DiscordMember user, string? name, int? limit);
    }
}
