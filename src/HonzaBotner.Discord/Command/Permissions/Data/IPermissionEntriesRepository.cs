using System.Collections.Generic;
using System.Threading.Tasks;

namespace HonzaBotner.Discord.Command.Permissions.Data
{
    public interface IPermissionEntriesRepository
    {
         Task<IList<IPermissionEntry>> GetPermissionEntriesByPermissionsAsync(IEnumerable<string> permissions);
    }
}
