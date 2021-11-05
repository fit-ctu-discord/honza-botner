using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Managers
{
    public interface IButtonManager
    {
        /// <summary>
        /// Sets up default buttons on verification messages
        /// </summary>
        /// <param name="target">Target message where the buttons will be added</param>
        Task SetupVerificationButtons(DiscordMessage target);

        /// <summary>
        /// Removes all button interactions from provided message
        /// </summary>
        /// <param name="target">Target message</param>
        /// <returns></returns>
        Task RemoveButtonsFromMessage(DiscordMessage target);
    }
}
