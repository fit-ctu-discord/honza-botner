using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Command;
using HonzaBotner.Services.Contract;

namespace HonzaBotner.Discord.Services.Commands
{
    public class AuthorizeCommand : IChatCommand
    {
        private readonly IAuthorizationService _authorizationService;
        private const string LinkTemplate = "https://localhost:5001/Auth/Authenticate/{0}";

        public const string ChatCommand = "authorize";

        public AuthorizeCommand(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public async Task<ChatCommendExecutedResult> ExecuteAsync(DiscordClient client, DiscordMessage message, CancellationToken cancellationToken = default)
        {
            DiscordUser user = message.Author;
            DiscordDmChannel channel = await client.CreateDmAsync(user);

            string? code = await _authorizationService.GetAuthorizationCodeAsync(message.Channel.GuildId, user.Id);
            if (code == null)
            {
                await message.RespondAsync("Already authorized");
                return ChatCommendExecutedResult.InternalError;
            }
            string link = string.Format(LinkTemplate, code);

            await channel.SendMessageAsync($"Hi, authorize by following this link: {link}");
            return ChatCommendExecutedResult.Ok;
        }
    }
}
