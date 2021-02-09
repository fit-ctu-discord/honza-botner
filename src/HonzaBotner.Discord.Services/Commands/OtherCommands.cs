using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Services.Commands
{
    public class OtherCommands : BaseCommandModule
    {
        [Command("hug"), Description("Hug someone who needs it")]
        public Task HugCommand(CommandContext ctx, [Description("Who should be hugged")] DiscordMember member)
        {
            throw new NotImplementedException();
        }

        [Command("hi")]
        public Task HiCommand(CommandContext ctx)
        {
            return ctx.RespondAsync("Hi!");
        }
    }
}
