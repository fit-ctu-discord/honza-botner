using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Emzi0767;
using HonzaBotner.Discord.Services.Extensions;

namespace HonzaBotner.Discord.Services.SlashCommands
{
    public class FunSCommands : ApplicationCommandModule
    {
        [SlashCommand("choose", "Let destiny decide")]
        public async Task ChooseAsync(InteractionContext ctx,
            [Option("Options", "Options separated by ';'")] string args)
        {
            SecureRandom random = new();
            string[] choices = args.Split(";");
            string choice = choices[random.Next(choices.Length)].RemoveDiscordMentions(ctx.Guild);

            string response = (choices.Length == 1 ? "How simple: " : "Destiny decided: ")
                              + $"**{choice}**";
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent(response));
        }

    }
}
