using System.Collections.Generic;
using System.Threading.Tasks;

namespace HonzaBotner.Discord.Command.Permissions.Repositories
{
    public interface IPermissionEntriesRepository
    {
         Task<List<PermissionEntry>> GetPermissionEntriesByPermissionsAsync(List<string> permissions);
    }
}
