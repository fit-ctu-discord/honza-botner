using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Commands.Polls;
using HonzaBotner.Discord.Services.Options;
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

        public PollCommands(IOptions<CommonCommandOptions> options)
        {
            _options = options.Value;
        }

        [GroupCommand]
        [Description("Creates either yes/no or ABC poll.")]
        [Priority(1)]
        public async Task PollCommandAsync(CommandContext ctx,
            [Description("Options for the pool.")] params string[] options)
        {
            if (options.Length == 0)
            {
                await ctx.RespondAsync("You have to ask a question");
                return;
            }

            IPoll poll;

            if (options.Length == 1)
            {
                poll = new YesNoPoll(ctx.Member.DisplayName, ctx.Member.AvatarUrl, options.First());
            }
            else
            {
                poll = new AbcPoll(ctx.Member.Mention, options.First(),
                    options.Skip(1).ToList());
            }

            try
            {
                await poll.PostAsync(ctx.Client, ctx.Channel);
                await ctx.Message.DeleteAsync();
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

        [Command("yesno")]
        [Description("Creates a yesno pool.")]
        [Priority(2)]
        public async Task YesNoPollCommand(CommandContext ctx,
            [RemainingText, Description("Poll's question.")]
            string question)
        {
            try
            {
                await new YesNoPoll(ctx.Member.DisplayName, ctx.Member.AvatarUrl, question)
                    .PostAsync(ctx.Client, ctx.Channel);
                await ctx.Message.DeleteAsync();
            }
            catch
            {
                await ctx.RespondAsync(PollErrorMessage);
            }
        }

        [Command("abc")]
        [Description("Creates an abc pool.")]
        [Priority(2)]
        public async Task AbcPollCommandAsync(CommandContext ctx,
            [Description("Poll's question.")] string question,
            [Description("Poll's options.")] params string[] answers)
        {
            try
            {
                await new AbcPoll(ctx.Member.Mention, question, answers.ToList())
                    .PostAsync(ctx.Client, ctx.Channel);
                await ctx.Message.DeleteAsync();
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

        [Command("add")]
        [Description("Adds options to an existing poll. You need to reference it via reply.")]
        [Priority(2)]
        public async Task AddAbcOptionAsync(CommandContext ctx,
            [Description("Additional options.")]
            params string[] options)
        {

            DiscordMessage? originalMessage = ctx.Message.ReferencedMessage;
            if (originalMessage is null)
            {
                await ctx.RespondAsync("You need to reply to the poll you want to edit.");
                return;
            }

            if (!originalMessage.Author.IsCurrent
                || (originalMessage.Embeds?.Count.Equals(0) ?? true)
                || !(originalMessage.Embeds[0].Footer?.Text.Equals("AbcPoll") ?? false))
            {
                await ctx.RespondAsync("The message you referenced is not an editable Poll." +
                                       "Please reference poll which was created by this bot and Abc type.");
                return;
            }

            DiscordEmbed originalPoll = originalMessage.Embeds[0];
            DiscordRole modRole = (await ctx.Client.GetGuildAsync(ctx.Guild.Id)).GetRole(_options.ModRoleId);

            // Extract original author ID via discord's mention format <@!123456789>
            string authorId = originalPoll.Description.Substring(
                originalPoll.Description.LastIndexOf("!", StringComparison.Ordinal) + 1);
            authorId = authorId.Remove(authorId.LastIndexOf(">", StringComparison.Ordinal));

            if (authorId != ctx.Member.Id.ToString() && !ctx.Member.Roles.Contains(modRole))
            {
                await ctx.RespondAsync("You are not authorized to edit this poll");
                return;
            }

            List<string> newOptions = options.ToList();
            List<string> oldOptions = originalPoll.Fields
                .Select(ef => ef.Value)
                .ToList();
            oldOptions.AddRange(newOptions);

            try
            {
                await new AbcPoll(ctx.Member.Mention, originalPoll.Title, oldOptions)
                    .UpdateAsync(ctx.Client, originalMessage);
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
