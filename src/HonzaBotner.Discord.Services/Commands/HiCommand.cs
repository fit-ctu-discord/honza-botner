using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Command;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Commands
{
    public class HiCommand : BaseCommand
    {
        public const string ChatCommand = "hi";

        public HiCommand(IPermissionHandler permissionHandler, ILogger<HiCommand> logger)
            : base(permissionHandler, logger)
        {
        }

        protected override async Task<ChatCommendExecutedResult> ExecuteAsync(DiscordClient client, DiscordMessage message, CancellationToken cancellationToken)
        {
            await client.SendMessageAsync(message.Channel, "Hi");

            return ChatCommendExecutedResult.Ok;
        }
    }
}
