using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Commands.Polls;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("poll")]
    [Description("Commands to create polls.")]
    [RequireGuild]
    public class PollCommands : BaseCommandModule
    {
        private const string PollErrorMessage =
            "Poll build failed. Make sure the question has less than 256 characters and each option has less than 1024 characters.";

        private readonly CommonCommandOptions _options;
        private readonly ILogger<PollCommands> _logger;

        public PollCommands(IOptions<CommonCommandOptions> options, ILogger<PollCommands> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        [GroupCommand]
        [Description("Creates either yes/no or ABC poll.")]
        [Priority(1)]
        public async Task PollCommandAsync(CommandContext ctx,
            [Description("Options for the pool.")] params string[] options)
        {
            switch (options.Length)
            {
                case 0:
                    await PollHelpAsync(ctx);
                    break;
                case 1:
                    await CreatePollAsync(ctx, options.First());
                    break;
                default:
                    await CreatePollAsync(ctx, options.First(), options.Skip(1).ToList());
                    break;
            }
        }

        [Command("yesno")]
        [Description("Creates a yesno pool.")]
        [Priority(2)]
        public async Task YesNoPollCommand(CommandContext ctx,
            [RemainingText, Description("Poll's question.")]
            string question)
        {
            await CreatePollAsync(ctx, question);
        }

        [Command("abc")]
        [Description("Creates an abc pool.")]
        [Priority(2)]
        public async Task AbcPollCommandAsync(CommandContext ctx,
            [Description("Poll's question.")] string question,
            [Description("Poll's options.")] params string[] answers)
        {
            await CreatePollAsync(ctx, question, answers.ToList());
        }

        private async Task CreatePollAsync(CommandContext ctx, string question, List<string>? answers = null)
        {
            Poll poll;
            if (answers is null) poll = new YesNoPoll(question);
            else poll = new AbcPoll(question, answers);

            try
            {
                await poll.PostAsync(ctx.Client, ctx.Channel, ctx.Member.Mention);
                await ctx.Message.DeleteAsync();
            }
            catch (ArgumentException e)
            {
                await ctx.RespondAsync(e.Message);
            }
            catch (Exception e)
            {
                await ctx.RespondAsync(PollErrorMessage);
                _logger.LogWarning(e, $"Failed to create new {poll.PollType}");
            }
        }

        private async Task PollHelpAsync(CommandContext ctx)
        {
            DiscordEmbed embed = new DiscordEmbedBuilder()
                .WithTitle("Polls")
                .WithDescription("Create unique polls using `poll` command." +
                                 " You can also add options to existing polls using `poll add`." +
                                 "\n\nBot currently supports following formats:")
                .AddField("YesNo Poll :+1:",
                    "Ask simple questions with binary answers. Bot will add :+1: and :-1: as answer options." +
                    "\nUsage: `poll yesno Is this feature cool?`")
                .AddField("Abc Poll :regional_indicator_a:",
                    "Basic options not enough? You can include your own answers." +
                    "\nUsage: `poll abc \"Which is the best?\" \"Dogs\" \"Cats\" \"Progtest\"`" +
                    "\nQuotation marks (\") are required in this command")
                .AddField("Editing existing poll",
                    "To add additional options to already existing **abc** poll that you've created, " +
                    "use subcommand 'add'. You have to send this command as a reply to the original poll." +
                    "\nUsage: `poll add \"Marast!\" \"Moodle\"`\n`\"`'s are again necessary")
                .WithColor(DiscordColor.Yellow)
                .Build();
            await ctx.RespondAsync(embed);
        }

        [Command("add")]
        [Description("Adds options to an existing poll. You need to reference it via reply.")]
        [Priority(2)]
        public async Task AddAbcOptionAsync(CommandContext ctx,
            [Description("Additional options.")]
            params string[] options)
        {
            DiscordMessage? originalMessage = ctx.Message.ReferencedMessage;

            if (originalMessage is null
                || !originalMessage.Author.IsCurrent
                || (originalMessage.Embeds?.Count.Equals(0) ?? true)
                || !(originalMessage.Embeds[0].Footer?.Text.Equals("AbcPoll") ?? false))
            {
                await PollHelpAsync(ctx);
                return;
            }

            DiscordEmbed originalPoll = originalMessage.Embeds[0];
            DiscordRole modRole = (await ctx.Client.GetGuildAsync(ctx.Guild.Id)).GetRole(_options.ModRoleId);

            // Extract original author Mention via discord's mention format <@!123456789>
            string authorMention = originalPoll.Description.Substring(
                originalPoll.Description.LastIndexOf("<", StringComparison.Ordinal));

            if (authorMention != ctx.Member.Mention && !ctx.Member.Roles.Contains(modRole))
            {
                await ctx.RespondAsync("You are not authorized to edit this poll");
                return;
            }

            try
            {
                await new AbcPoll(originalMessage).AddOptionsAsync(ctx.Client, authorMention, options.ToList());
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":+1:"));
            }
            catch (ArgumentException e)
            {
                await ctx.RespondAsync(e.Message);
            }
            catch
            {
                await ctx.RespondAsync(PollErrorMessage);
            }
        }
    }
}
