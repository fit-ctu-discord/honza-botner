using System.Threading.Tasks;
using DSharpPlus;

namespace HonzaBotner.Discord.Managers;

public interface ISlashManager
{
    Task UpdateStartupPermissions();
}
