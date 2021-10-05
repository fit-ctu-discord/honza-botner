using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace HonzaBotner.Discord.Services.Attributes
{
    public class SlashRequireRoleAttribute : SlashCheckBaseAttribute
    {

        public ulong roleId;

        public SlashRequireRoleAttribute(ulong role)
        {
            roleId = role;
        }

        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            if (ctx.Member is null) return false;

            foreach (DiscordRole role in ctx.Member.Roles)
            {
                if (role.Id == roleId) return true;
            }

            return false;
        }
    }
}
