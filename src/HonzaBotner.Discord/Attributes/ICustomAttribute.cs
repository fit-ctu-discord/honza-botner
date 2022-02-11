using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Attributes;

public interface ICustomAttribute
{
    public Task<DiscordEmbed> BuildFailedCheckDiscordEmbed();
}
