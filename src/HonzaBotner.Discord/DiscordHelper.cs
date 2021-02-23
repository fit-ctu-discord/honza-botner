using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord
{
    public static class DiscordHelper
    {
        public static string GetMention(ulong authorId)
        {
            return $"<@{authorId}>";
        }

        public static string GetChannel(ulong channelId)
        {
            return $"<#{channelId}>";
        }

        public static string GetMessageLink(ulong guild, ulong channel, ulong message)
        {
            return $"https://discordapp.com/channels/{guild}/{channel}/{message}";
        }

        /// <summary>
        /// Finds DiscordMessage from Discord link to message.
        /// </summary>
        /// <param name="guild">Discord guild to find the message at.</param>
        /// <param name="link">URL of the message.</param>
        /// <returns>DiscordMessage if successful, otherwise null.</returns>
        public static async Task<DiscordMessage?> FindMessageFromLink(DiscordGuild guild, string link)
        {
            // Match the channel and message IDs.
            const string pattern = @"https://discord(?:app)?\.com/channels/(?:\d+)/(\d+)/(\d+)/?";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(link);

            // Malformed message link.
            if (!match.Success) return null;

            try
            {
                bool channelParseSuccess = ulong.TryParse(match.Groups[1].Value, out ulong channelId);
                if (!channelParseSuccess) return null;

                DiscordChannel? channel = guild.GetChannel(channelId);

                if (channel.Type != ChannelType.Text) return null;

                bool messageParseSuccess = ulong.TryParse(match.Groups[2].Value, out ulong messageId);
                if (!messageParseSuccess) return null;

                return await channel.GetMessageAsync(messageId);
            }
            catch
            {
                return null;
            }
        }
    }
}
