using System.Threading.Tasks;
using HonzaBotner.Discord;

namespace HonzaBotner.Services
{
    public class  AllowAllPermissionHandler : IPermissionHandler
    {
        public Task<bool> HasRightsAsync(ulong userId, CommandPermission requiredPermission)
            => Task.FromResult(true);
    }
}
