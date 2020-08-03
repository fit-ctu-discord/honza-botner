using System.Collections.Generic;
using System.Threading.Tasks;

namespace HonzaBotner.Discord.Command.Permissions.Data
{
    public interface IPermissionEntriesRepository
    {
         Task<IList<PermissionEntry>> GetPermissionEntriesByPermissionsAsync(IEnumerable<string> permissions);
    }
}
