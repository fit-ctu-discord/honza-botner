using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace HonzaBotner.Discord
{
    public interface IVoiceManager
    {
        Task Run();

        Task AddNewVoiceChannelAsync(DiscordClient client, DiscordChannel channelToCloneFrom, DiscordMember user, string? name, int? limit);
    }
}
