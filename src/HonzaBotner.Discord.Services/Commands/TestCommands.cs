using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using HonzaBotner.Discord.Services.Attributes;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("test")]
    [Description("Testing commands")]
    [RequireMod]
    public class TestCommands : BaseCommandModule
    {
        [Command("boostCount")]
        public async Task BoostCount(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(ctx.Guild.PremiumSubscriptionCount.ToString());
        }
    }
}
