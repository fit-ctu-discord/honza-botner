using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using HonzaBotner.Discord.Services.Extensions;

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
        var answers = options.Split(delimiter, StringSplitOptions.TrimEntries & StringSplitOptions.RemoveEmptyEntries)
            .Select(option => option.Trim().RemoveDiscordMentions(ctx.Guild))
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
        text.Append("`" + winner + "`");
        if (answers.Length > 1)
        {
            text.Append("\nOptions were:\n");
            foreach (var option in answers)
            {
                text.Append("`" + option+ "`, ");
            }

            text.Remove(text.Length - 2, 2);
        }
        else
        {
            text.Append("\nAre you bipolar or why was this necessary?!");
        }

        await ctx.CreateResponseAsync(text.ToString());
    }

}
