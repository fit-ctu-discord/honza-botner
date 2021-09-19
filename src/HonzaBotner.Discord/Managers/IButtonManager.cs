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
        Task SetupButtons();
    }
}
