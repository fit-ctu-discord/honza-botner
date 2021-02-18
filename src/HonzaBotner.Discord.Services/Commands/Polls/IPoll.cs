using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Services.Commands.Polls
{
    public interface IPoll
    {
        Task PostAsync(DiscordClient client, DiscordChannel channel);
    }
}
