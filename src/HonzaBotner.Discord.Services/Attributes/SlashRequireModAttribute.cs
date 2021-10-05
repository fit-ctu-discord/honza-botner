using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Attributes
{
    public class SlashRequireModAttribute : SlashCheckBaseAttribute
    {
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            CommonCommandOptions? options =
                ctx.Services.GetService<IOptions<CommonCommandOptions>>()?.Value;
            IGuildProvider? guildProvider = ctx.Services.GetService<IGuildProvider>();
            if (guildProvider is null) return false;
            DiscordGuild guild = await guildProvider.GetCurrentGuildAsync();
            DiscordMember member = await guild.GetMemberAsync(ctx.User.Id);
            return member.Roles.Contains(guild.GetRole(options?.ModRoleId ?? 0));
        }
    }
}
