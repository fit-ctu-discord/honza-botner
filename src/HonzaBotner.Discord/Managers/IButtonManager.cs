using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Managers
{
    public interface IButtonManager
    {
        /// <summary>
        /// Sets up default buttons on verification messages
        /// </summary>
        /// <param name="guilds">Dictionary of GuildID and DiscordGuild the bot is connected to</param>
        Task SetupButtons(IReadOnlyDictionary<ulong, DiscordGuild> guilds);
    }
}
