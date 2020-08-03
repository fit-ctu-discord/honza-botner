using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Command.Permissions
{
    internal interface IPermissionResolver
    {
        Task<bool> IsUserEligibleToRunAsync(DiscordMember user, IChatCommand command);

        Task<bool> IsUserEligibleToRunAsync(DiscordMember user, IEnumerable<string> requiredPermissions);
    }
}
