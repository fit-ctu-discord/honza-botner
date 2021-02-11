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
        private readonly IUrlProvider _urlProvider;
        private readonly IAuthorizationService _authorizationService;

        public AuthorizeCommands(IUrlProvider urlProvider, IAuthorizationService authorizationService)
        {
            _urlProvider = urlProvider;
            _authorizationService = authorizationService;
        }

        [Command("authorize"), Aliases("auth")]
        public async Task AuthorizeCommand(CommandContext ctx)
        {
            DiscordMessage message = ctx.Message;
            DiscordUser user = message.Author;
            DiscordDmChannel channel = await message.Channel.Guild.Members[user.Id].CreateDmChannelAsync();

            if (await _authorizationService.IsUserVerified(user.Id))
            {
                await channel.SendMessageAsync($"You are already authorized");
            }
            else
            {
                string link = _urlProvider.GetAuthLink(user.Id);
                await channel.SendMessageAsync($"Hi, authorize by following this link: {link}");
            }
        }
    }
}
