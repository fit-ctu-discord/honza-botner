using System.Collections.Generic;
using System.Threading.Tasks;

namespace HonzaBotner.Services.Contract
{
    public interface IDiscordRoleManager
    {
        HashSet<DiscordRole> MapUsermapRoles(params string[] kosRoles);

        Task<bool> GrantRolesAsync(ulong guildId, ulong userId, IEnumerable<DiscordRole> discordRoles);
    }
}
