using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Attributes;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Attributes
{
    public class RequireModAttribute : CheckBaseAttribute, IRequireModAttribute
    {
        public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            CommonCommandOptions? options =
                ctx.CommandsNext.Services.GetService<IOptions<CommonCommandOptions>>()?.Value;
            IGuildProvider? guildProvider = ctx.Services.GetService<IGuildProvider>();
            if (guildProvider is null) return false;
            DiscordGuild guild = await guildProvider.GetCurrentGuildAsync();
            DiscordMember member = await guild.GetMemberAsync(ctx.User.Id);
            return member.Roles.Contains(guild.GetRole(options?.ModRoleId ?? 0));
        }
    }
}
