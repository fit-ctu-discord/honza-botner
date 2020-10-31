using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Command;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Commands
{
    public class HugCommand : BaseCommand
    {
        public const string ChatCommand = "hug";
        // ;hug <user-mention>

        protected override bool CanBotExecute => false;
        protected override CommandPermission RequiredPermission => CommandPermission.Authorized;

        protected readonly string _hugEmoteName;

        public HugCommand(IPermissionHandler permissionHandler, ILogger<HugCommand> logger, IOptions<CommonCommandOptions> options)
            : base(permissionHandler, logger)
        {
            _hugEmoteName = options.Value.HugEmoteName;
        }

        protected override async Task<ChatCommendExecutedResult> ExecuteAsync(DiscordClient client,
            DiscordMessage message, CancellationToken cancellationToken = default)
        {
            if (message.Content.Split(" ").Length != 2) return ChatCommendExecutedResult.WrongSyntax;
            if (message.MentionedUsers.Count != 1) return ChatCommendExecutedResult.WrongSyntax;

            await client.SendMessageAsync(message.Channel, $":{_hugEmoteName}: {message.MentionedUsers[0].Mention}");

            return ChatCommendExecutedResult.Ok;
        }
    }
}
