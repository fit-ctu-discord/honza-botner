using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Command;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Commands.Muting
{
    public class MuteCommand : BaseCommand
    {
        private readonly IGuildProvider _guildProvider;

        public const string ChatCommand = "mute";
        // ;mute <user-mention>

        protected override CommandPermission RequiredPermission => CommandPermission.Mod;

        public MuteCommand(IPermissionHandler permissionHandler, ILogger<MuteCommand> logger,
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

            if (MuteRoleHelper.IsMuted(targetMember))
            {
                await message.RespondAsync("Target user is already muted");
                return ChatCommendExecutedResult.Ok;
            }

            var guild = await _guildProvider.GetCurrentGuildAsync();
            await MuteRoleHelper.Mute(guild, targetMember);

            return ChatCommendExecutedResult.Ok;
        }
    }
}
