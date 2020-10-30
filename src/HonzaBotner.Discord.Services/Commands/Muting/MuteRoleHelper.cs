using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Services.Commands.Muting
{
    public static class MuteRoleHelper
    {
        private const ulong MuteRoleId = 750276752663904306;

        public static bool IsMuted(DiscordMember user)
        {
            return user.Roles.Any(r => r.Id == MuteRoleId);
        }

        public static async Task Mute(DiscordGuild guild, DiscordMember user)
        {
            var muteRole = GetMuteRole(guild);
            if (muteRole == null) return;

            await guild.GrantRoleAsync(user, muteRole, "User muted");
        }

        public static async Task Unmute(DiscordGuild guild, DiscordMember user)
        {
            var muteRole = GetMuteRole(guild);
            if (muteRole == null) return;

            await guild.RevokeRoleAsync(user, muteRole, "User unmuted");
        }

        public static DiscordRole? GetMuteRole(DiscordGuild guild)
        {
            return guild.Roles.FirstOrDefault(r => r.Id == MuteRoleId);
        }
    }
}
