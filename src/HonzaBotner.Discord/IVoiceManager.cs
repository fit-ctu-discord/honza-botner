using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace HonzaBotner.Discord
{
    public interface IVoiceManager
    {
        Task Run(CancellationToken cancellationToken);

        Task Client_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs args);
    }
}
