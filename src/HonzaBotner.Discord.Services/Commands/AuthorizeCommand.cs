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
        private readonly IAuthorizationService _authorizationService;
        private const string LinkTemplate = "https://localhost:5001/Auth/Authenticate/{0}";

        public const string ChatCommand = "authorize";

        public AuthorizeCommand(IAuthorizationService authorizationService,
            IPermissionHandler permissionHandler, ILogger<AuthorizeCommand> logger) : base(permissionHandler, logger)
        {
            _authorizationService = authorizationService;
        }

        protected override async Task<ChatCommendExecutedResult> ExecuteAsync(DiscordClient client,
            DiscordMessage message, CancellationToken cancellationToken = default)
        {
            DiscordUser user = message.Author;
            DiscordDmChannel channel = await client.CreateDmAsync(user);

            string? code = await _authorizationService.GetAuthorizationCodeAsync(user.Id);
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
