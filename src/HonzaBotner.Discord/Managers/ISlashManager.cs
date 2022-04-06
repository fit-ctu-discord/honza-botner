using System.Threading.Tasks;

namespace HonzaBotner.Discord.Managers;

public interface ISlashManager
{
    Task UpdateStartupPermissions();
}
