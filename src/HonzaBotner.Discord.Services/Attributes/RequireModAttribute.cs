using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Attributes
{
    public class RequireModAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            CommonCommandOptions? options =
                ctx.CommandsNext.Services.GetService<IOptions<CommonCommandOptions>>()?.Value;
            return Task.FromResult(ctx.Member.Roles.Contains(ctx.Guild.GetRole(options?.ModRoleId ?? 0)));
        }
    }
}
