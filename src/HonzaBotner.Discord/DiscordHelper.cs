namespace HonzaBotner.Discord
{
    public static class DiscordHelper
    {
        public static string GetMention(ulong authorId)
        {
            return $"<@{authorId}>";
        }

        public static string GetMessageLink(ulong guild, ulong channel, ulong message)
        {
            return $"https://discordapp.com/channels/{guild}/{channel}/{message}";
        }
    }
}
