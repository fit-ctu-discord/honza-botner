using System.Threading.Tasks;

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
