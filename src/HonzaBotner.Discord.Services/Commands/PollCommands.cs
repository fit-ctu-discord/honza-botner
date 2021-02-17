using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using HonzaBotner.Discord.Services.Commands.Polls;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("poll")]
    [Description("Commands to create polls.")]
    public class PollCommands : BaseCommandModule
    {
        [GroupCommand]
        [Description("Creates either yes/no or ABC poll.")]
        [Priority(1)]
        public async Task PollCommand(CommandContext ctx,
            [Description("Options for the pool.")] params string[] options)
        {
            if (options.Length == 0)
            {
                await ctx.RespondAsync($"You have to ask a question");
                return;
            }

            IPoll poll;

            if (options.Length == 1)
            {
                poll = new YesNoPoll(ctx.Member.RatherNicknameThanUsername(), ctx.Member.AvatarUrl, options.First());
            }
            else if (options.Length - 1 <= AbcPoll.MaxOptions)
            {
                poll = new AbcPoll(ctx.Member.RatherNicknameThanUsername(), ctx.Member.AvatarUrl, options.First(),
                    options.Skip(1).ToList());
            }
            else
            {
                await ctx.RespondAsync($"Too many options, maximum number of responses is ${AbcPoll.MaxOptions}.");
                return;
            }

            await poll.Post(ctx.Client, ctx.Channel);
        }

        [Command("yesno")]
        [Description("Creates a yesno pool.")]
        [Priority(2)]
        public async Task YesnoPollCommand(CommandContext ctx,
            [RemainingText, Description("Poll's question.")]
            string question)
        {
            await new YesNoPoll(ctx.Member.RatherNicknameThanUsername(), ctx.Member.AvatarUrl, question)
                .Post(ctx.Client, ctx.Channel);
        }

        [Command("abc")]
        [Description("Creates an abc pool.")]
        [Priority(2)]
        public async Task AbcPollCommand(CommandContext ctx,
            [Description("Poll's question.")] string question,
            [Description("Poll's options.")] params string[] answers)
        {
            await new AbcPoll(ctx.Member.RatherNicknameThanUsername(), ctx.Member.AvatarUrl, question, answers.ToList())
                .Post(ctx.Client, ctx.Channel);
        }
    }
}
