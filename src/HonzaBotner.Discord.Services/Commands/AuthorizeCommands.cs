using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Services.Contract;

namespace HonzaBotner.Discord.Services.Commands
{
    [Description("Příkazy sloužící k ověření totožnosti")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class AuthorizeCommands : BaseCommandModule
    {
        public IUrlProvider UrlProvider { private get; set; } = null!;
        public IAuthorizationService AuthorizationService { private get; set; } = null!;

        [Command("authorize"), Aliases("auth")]
        public async Task AuthorizeCommand(CommandContext ctx)
        {
            DiscordMessage message = ctx.Message;
            DiscordUser user = message.Author;
            DiscordDmChannel channel = await message.Channel.Guild.Members[user.Id].CreateDmChannelAsync();

            if (await AuthorizationService.IsUserVerified(user.Id))
            {
                await channel.SendMessageAsync($"You are already authorized");
            }
            else
            {
                string link = UrlProvider.GetAuthLink(user.Id);
                await channel.SendMessageAsync($"Hi, authorize by following this link: {link}");
            }
        }
    }
}
