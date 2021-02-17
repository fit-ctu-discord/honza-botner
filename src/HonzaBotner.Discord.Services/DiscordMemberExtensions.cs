using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Services
{
    public static class DiscordMemberExtensions
    {
        public static string RatherNicknameThanUsername(this DiscordMember member)
        {
            return member.Nickname ?? member.Username;
        }
    }
}
