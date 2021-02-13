using System;
using System.Collections.Generic;
using System.Linq;
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

        [Command("newVoice")]
        public async Task NewVoice(CommandContext ctx)
        {
            IEnumerable<DiscordChannel> others = ctx.Guild.GetChannel(750055929340231714).Children
                .Where(channel => channel.Id != 750055929340231716);
            foreach (DiscordChannel discordChannel in others)
            {
                if (!discordChannel.Users.Any())
                {
                    await discordChannel.DeleteAsync();
                }
            }

            DiscordChannel cloned = await ctx.Guild.GetChannel(750055929340231716).CloneAsync("Creates custom voice channel.");
            await cloned.ModifyAsync(model => model.Name = "New channel from user " + ctx.User.Username);
            await cloned.PlaceMemberAsync(await ctx.Guild.GetMemberAsync(ctx.User.Id));
        }
    }
}
