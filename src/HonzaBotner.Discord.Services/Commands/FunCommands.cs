using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;

namespace HonzaBotner.Discord.Services.Commands;

public class FunCommands : ApplicationCommandModule
{

    [SlashCommand("choose", "Pick one of provided options")]
    public async Task ChooseCommandAsync(
        InteractionContext ctx,
        [Option("options", "Options to choose from. Default delimiter: ,")]
        string options,
        [Option("delimiter", "Character that separates options. Default: \",\"")]
        [MinimumLength(1), MaximumLength(1)]
        string delimiter = ","
        )
    {
        var answers = options.Split(delimiter)
            .Select(option => option.Trim())
            .Where(option => option != "").ToArray();
        if (answers.Length == 0)
        {
            await ctx.CreateResponseAsync("Nope");
            return;
        }
        Random random = new();
        var text = new StringBuilder("I picked: ");
        var winNumber = random.Next(answers.Length);
        var winner = answers[winNumber];
        text.Append( winner);
        if (answers.Length > 1)
        {
            text.Append("\nOptions were:\n");
            foreach (var option in answers)
            {
                text.Append(option+ ", ");
            }

            text.Remove(text.Length - 2, 2);
        }
        else
        {
            text.Append("\nPlease separate the options by `,`");
        }

        await ctx.CreateResponseAsync(text.ToString());
    }

}
