using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using HonzaBotner.Discord.Services.Extensions;

namespace HonzaBotner.Discord.Services.SCommands;

public class FunCommands : ApplicationCommandModule
{

    [SlashCommand("choose", "Pick one of provided options")]
    public async Task ChooseCommandAsync(InteractionContext ctx)
    {
        string modalId = $"id-funChoose-{ctx.User.Id}";
        var modal = new DiscordInteractionResponseBuilder()
            .WithTitle("I can choose something!")
            .WithCustomId(modalId)
            .AddComponents(new TextInputComponent("Entries on separate lines", modalId, style: TextInputStyle.Paragraph, min_length: 1));

        await ctx.CreateResponseAsync(InteractionResponseType.Modal, modal);

        var response = await ctx.Client
            .GetInteractivity()
            .WaitForModalAsync(modalId, TimeSpan.FromMinutes(2));
        if (!response.TimedOut)
        {
            var answers = response.Result.Values[modalId].Split('\n').Where(line => line.Trim() != "").ToArray();
            Random random = new();
            var text = new StringBuilder("I picked: ");
            string winner = answers[random.Next(answers.Length)];
            text.Append("`" + winner + "`");
            if (answers.Length > 1)
            {
                text.Append("\nOther options were:\n");
                foreach (var option in answers.Where(option => option != winner))
                {
                    text.Append("`" + option + "`" + '\n');
                }
            }
            await response.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent(text.ToString().RemoveDiscordMentions(ctx.Guild)));
        }
    }

}
