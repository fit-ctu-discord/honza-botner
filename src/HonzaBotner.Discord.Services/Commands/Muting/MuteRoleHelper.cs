using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Commands.Muting
{
    public class MuteRoleHelper
    {
        private readonly ulong _muteRoleId;
        public MuteRoleHelper(IOptions<CommonCommandOptions> options)
        {
            _muteRoleId = options.Value.MuteRoleId;
        }

        public bool IsMuted(DiscordMember user)
        {
            return user.Roles.Any(r => r.Id == _muteRoleId);
        }

        public async Task Mute(DiscordGuild guild, DiscordMember user)
        {
            var muteRole = GetMuteRole(guild);
            if (muteRole == null) return;

            await guild.GrantRoleAsync(user, muteRole, "User muted");
        }

        public async Task Unmute(DiscordGuild guild, DiscordMember user)
        {
            var muteRole = GetMuteRole(guild);
            if (muteRole == null) return;

            await guild.RevokeRoleAsync(user, muteRole, "User unmuted");
        }

        public DiscordRole? GetMuteRole(DiscordGuild guild)
        {
            return guild.Roles.FirstOrDefault(r => r.Id == _muteRoleId);
        }
    }
}
