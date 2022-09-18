using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord;

public interface IGuildProvider
{
    public ulong GuildId { get; }
    Task<DiscordGuild> GetCurrentGuildAsync();
}
