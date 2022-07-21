using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HonzaBotner.Discord.Services.Commands.Polls;
using HonzaBotner.Discord.Services.Extensions;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Commands;

[SlashCommandGroup("poll", "Create different polls")]
public class PollCommands : ApplicationCommandModule
{
    private const string PollErrorMessage =
        "Poll build failed. Make sure the question has less than 256 characters and each option has less than 1024 characters.";

    private readonly CommonCommandOptions _options;
    private readonly ILogger<PollCommands> _logger;
    private readonly IGuildProvider _guildProvider;

    public PollCommands(IOptions<CommonCommandOptions> options, ILogger<PollCommands> logger,
        IGuildProvider guildProvider)
    {
        _options = options.Value;
        _logger = logger;
        _guildProvider = guildProvider;
    }

    [SlashCommand("yesno", "Create poll with yes/no answers.")]
    public async Task YesNoPollCommandAsync(
        InteractionContext ctx,
        [Option("question", "Question/query you want to vote about.")] string question
    )
    {
        await CreateDefaultPollAsync(ctx, question);
    }

    [SlashCommand("abc", "Create poll with custom answers")]
    public async Task AbcPollCommandAsync(
        InteractionContext ctx,
        [Option("question", "Question/query you want to vote about.")]
        string question,
        [Option("answers", "Answers separated by ','")]
        string answers
    )
    {
        await CreateDefaultPollAsync(ctx, question,
            answers.Split(',')
                .Select(answer => answer.Trim().RemoveDiscordMentions())
                .Where(answer => answer != "").ToList());
    }

    private async Task CreateDefaultPollAsync(InteractionContext ctx, string question, List<string>? answers = null)
    {
        try
        {
            Poll poll = answers is null
                ? new YesNoPoll(ctx.Member.Mention, question)
                : new AbcPoll(ctx.Member.Mention, question, answers);

            await poll.PostAsync(ctx.Client, ctx.Channel);
            await ctx.CreateResponseAsync("Poll created", true);
        }
        catch (PollException e)
        {
            await ctx.CreateResponseAsync(e.Message, true);
        }
        catch (Exception e)
        {
            await ctx.CreateResponseAsync(PollErrorMessage, true);
            _logger.LogWarning(e, "Failed to create new Poll");
        }
    }


    [SlashCommand("add-answers", "Add answers to polls created by you")]
    public async Task AddPollOptionAsync(
        InteractionContext ctx,
        [Option("poll", "Link to original poll (Right click -> Copy Message Link).")] string link,
        [Option("answers", "Answers separated by ','")] string answers
    )
    {
        DiscordMessage? originalMessage = await DiscordHelper.FindMessageFromLink(ctx.Guild, link);
        List<string> options = answers.Split(',')
            .Select(answer => answer.Trim().RemoveDiscordMentions())
            .Where(answer => answer != "").ToList();

        if (originalMessage is null
            || options.Count == 0
            || !originalMessage.Author.IsCurrent
            || (originalMessage.Embeds?.Count.Equals(0) ?? true)
            || !(originalMessage.Embeds[0].Footer?.Text.Equals("AbcPoll") ?? false))
        {
            await ctx.CreateResponseAsync(
                "Original message is unreachable or it is not AbcPoll created by this bot.", true);
            return;
        }

        try
        {
            DiscordRole modRole = (await _guildProvider.GetCurrentGuildAsync()).GetRole(_options.ModRoleId);

            AbcPoll poll = new (originalMessage);

            if (poll.AuthorMention != ctx.Member?.Mention && !(ctx.Member?.Roles.Contains(modRole) ?? false))
            {
                await ctx.CreateResponseAsync("You are not authorized to edit this poll.", true);
                return;
            }

            await poll.AddOptionsAsync(ctx.Client, options);
            await ctx.CreateResponseAsync("Added new options to the poll.");
        }
        catch (PollException e)
        {
            await ctx.CreateResponseAsync(e.Message, true);
        }
        catch (Exception e)
        {
            await ctx.CreateResponseAsync(PollErrorMessage, true);
            _logger.LogWarning(e, "Failed to add options to abc poll");
        }
    }

}
