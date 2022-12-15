using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
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
            await ctx.CreateResponseAsync("Nope", true);
            return;
        }

        var response = new DiscordEmbedBuilder()
            .WithAuthor(name: ctx.Member.DisplayName, iconUrl: ctx.User.AvatarUrl)
            .WithColor(DiscordColor.DarkGreen);

        Random random = Random.Shared;

        var text = new StringBuilder($"I picked: {answers[random.Next(answers.Length)]}");
        if (ctx.User.Id is 470490558713036801 or 302127992258428929)
        {
            response
                .WithFooter("uwu", "https://cdn.discordapp.com/emojis/945812225208234066.png")
                .WithColor(DiscordColor.HotPink);
        }

        if (answers.Length > 1)
        {
            text.AppendLine("\n\nOptions were:");
            text.AppendJoin("\n", answers);
            response.WithDescription(text.ToString());
            await ctx.CreateResponseAsync(response.Build());
        }
        else
        {
            await ctx.CreateResponseAsync(response.WithDescription(text.ToString()).Build());
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .WithContent("Please separate the options by `,`")
                .AsEphemeral());
        }
    }

    [SlashCommand("SI1", "Vent frustration from BI-SI1")]
    public async Task SI1CommandAsync(InteractionContext ctx) {
        var response = new DiscordInteractionResponseBuilder()
            .WithContent("SIčka jsou ten nejvíc uwu předmět co kdy na FITu byl <3");

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
    }
}
