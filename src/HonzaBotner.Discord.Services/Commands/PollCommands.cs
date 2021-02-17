using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using HonzaBotner.Discord.Services.Commands.Polls;

namespace HonzaBotner.Discord.Services.Commands
{
    public class PollCommands : BaseCommandModule
    {
        [Command("poll"), Description("Creates either yes/no or ABC poll")]
        public async Task PollCommand(CommandContext ctx, params string[] options)
        {
            if (options.Length == 0)
            {
                await ctx.RespondAsync($"You have to ask a question");
                return;
            }

            IPoll poll;

            if (options.Length == 1)
            {
                poll = new YesNoPoll(ctx.Member.Username, ctx.Member.AvatarUrl, options.First());
            }
            else if (options.Length - 1 <= AbcPoll.MaxResponses)
            {
                poll = new AbcPoll(ctx.Member.Username, ctx.Member.AvatarUrl, options.First(), options.Skip(1).ToList());
            }
            else
            {
                await ctx.RespondAsync($"Too many responses, maximum number of responses is ${AbcPoll.MaxResponses}");
                return;
            }

            await poll.Post(ctx.Client, ctx.Channel);
        }
    }
}
