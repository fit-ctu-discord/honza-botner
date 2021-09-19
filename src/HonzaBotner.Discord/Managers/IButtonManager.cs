using System.Threading.Tasks;
using DSharpPlus.EventArgs;

namespace HonzaBotner.Discord.Managers
{
    public interface IButtonManager
    {
        Task SetupButtons(GuildDownloadCompletedEventArgs args);
    }
}
