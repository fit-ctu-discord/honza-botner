using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Command;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Commands
{
    public class AuthorizeCommand : BaseCommand
    {
        private readonly IUrlProvider _urlProvider;
        private readonly IAuthorizationService _authorizationService;
        private const string LinkTemplate = "https://localhost:5001/Auth/Authenticate/{0}";

        public const string ChatCommand = "authorize";

        public AuthorizeCommand(IUrlProvider urlProvider, IAuthorizationService authorizationService,
            IPermissionHandler permissionHandler, ILogger<AuthorizeCommand> logger) : base(permissionHandler, logger)
        {
            _urlProvider = urlProvider;
            _authorizationService = authorizationService;
        }

        protected override async Task<ChatCommendExecutedResult> ExecuteAsync(DiscordClient client,
            DiscordMessage message, CancellationToken cancellationToken = default)
        {
            DiscordUser user = message.Author;
            DiscordDmChannel channel = await client.CreateDmAsync(user);

            if (await _authorizationService.IsUserVerified(user.Id))
            {
                await channel.SendMessageAsync($"You are already authorized");
            }
            else
            {
                string link = _urlProvider.GetAuthLink(user.Id);
                await channel.SendMessageAsync($"Hi, authorize by following this link: {link}");
            }

            return ChatCommendExecutedResult.Ok;
        }
    }
}
