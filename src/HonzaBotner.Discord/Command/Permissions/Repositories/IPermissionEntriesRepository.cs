using System.Collections.Generic;
using System.Threading.Tasks;
using HonzaBotner.Discord.Command.Permissions.Entities;

namespace HonzaBotner.Discord.Command.Permissions.Repositories
{
    public interface IPermissionEntriesRepository
    {
         Task<List<PermissionEntry>> GetPermissionEntriesByPermissionsAsync(List<string> permissions);
    }
}
