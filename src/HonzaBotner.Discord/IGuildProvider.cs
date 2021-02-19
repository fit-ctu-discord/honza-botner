using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord
{
    public interface IGuildProvider
    {
        Task<DiscordGuild> GetCurrentGuildAsync();
    }
}
