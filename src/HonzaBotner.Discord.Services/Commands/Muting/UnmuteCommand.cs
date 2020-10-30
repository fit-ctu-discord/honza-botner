using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Command;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Commands.Muting
{
    public class UnmuteCommand : BaseCommand
    {
        public const string ChatCommand = "unmute";
        // ;unmute <user-mention>

        protected override CommandPermission RequiredPermission => CommandPermission.Mod;

        private readonly IGuildProvider _guildProvider;

        public UnmuteCommand(IPermissionHandler permissionHandler, ILogger<UnmuteCommand> logger,
            IGuildProvider guildProvider)
            : base(permissionHandler, logger)
        {
            _guildProvider = guildProvider;
        }

        protected override async Task<ChatCommendExecutedResult> ExecuteAsync(DiscordClient client,
            DiscordMessage message,
            CancellationToken cancellationToken = default)
        {
            if (message.Content.Split(" ").Length != 2) return ChatCommendExecutedResult.WrongSyntax;
            if (message.MentionedUsers.Count != 1) return ChatCommendExecutedResult.WrongSyntax;

            var targetUser = message.MentionedUsers[0];

            if (!(targetUser is DiscordMember targetMember))
            {
                return ChatCommendExecutedResult.InternalError;
            }

            if (!MuteRoleHelper.IsMuted(targetMember))
            {
                await message.RespondAsync("Target user is not muted");
                return ChatCommendExecutedResult.Ok;
            }

            var guild = await _guildProvider.GetCurrentGuildAsync();
            await MuteRoleHelper.Unmute(guild, targetMember);

            return ChatCommendExecutedResult.Ok;
        }
    }
}
