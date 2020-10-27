using System.Threading.Tasks;
using HonzaBotner.Database;
using HonzaBotner.Discord;
using Microsoft.EntityFrameworkCore;

namespace HonzaBotner.Services
{
    public class AuthorizePermissionHandler : IPermissionHandler
    {
        private readonly HonzaBotnerDbContext _dbContext;
        private readonly IPermissionHandler _permissionHandler;

        public AuthorizePermissionHandler(HonzaBotnerDbContext dbContext, IPermissionHandler permissionHandler)
        {
            _dbContext = dbContext;
            _permissionHandler = permissionHandler;
        }

        public async Task<bool> HasRightsAsync(ulong userId, CommandPermission requiredPermission)
        {
            if (requiredPermission == CommandPermission.Authorized)
            {
                return await _dbContext.Verifications.AnyAsync(v => v.UserId == userId);
            }

            return await _permissionHandler.HasRightsAsync(userId, requiredPermission);
        }
    }
}
