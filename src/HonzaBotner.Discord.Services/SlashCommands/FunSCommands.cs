using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace HonzaBotner.Discord.Services.SlashCommands
{
    public class FunSCommands : ApplicationCommandModule
    {
        [SlashCommand("ping", "pong?")]
        public async Task Ping(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Pong"));
        }
        
    }
}