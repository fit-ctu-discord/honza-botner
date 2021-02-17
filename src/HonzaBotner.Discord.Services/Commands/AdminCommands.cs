using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Attributes;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("admin")]
    [Description("Administrativní příkazy")]
    [Hidden]
    [RequireMod]
    public class AdminCommands : BaseCommandModule
    {
        [Command("send")]
        public async Task SendMessage(CommandContext ctx, DiscordChannel channel, [RemainingText] string text)
        {
            await channel.SendMessageAsync(text);
        }

        [Command("edit")]
        public async Task EditMessage(CommandContext ctx, string url, [RemainingText] string newText)
        {
            DiscordMessage? oldMessage = await DiscordHelper.FindMessageFromLink(ctx.Message.Channel.Guild, url);

            if (oldMessage == null)
            {
                // TODO
                return;
            }

            await oldMessage.ModifyAsync(newText);
        }

        [Command("countRole")]
        public async Task CountRole(CommandContext ctx, DiscordRole role)
        {
            int count = 0;

            foreach ((_, DiscordMember member) in ctx.Guild.Members)
            {
                if (member.Roles.Contains(role))
                {
                    count++;
                }
            }

            await ctx.Channel.SendMessageAsync(count.ToString());
        }
    }
}
