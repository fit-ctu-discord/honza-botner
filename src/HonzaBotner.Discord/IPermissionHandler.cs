using System.Threading.Tasks;

namespace HonzaBotner.Discord
{
    public interface IPermissionHandler
    {
        Task<bool> HasRightsAsync(ulong userId, CommandPermission requiredPermission);
    }
}
