using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace HonzaBotner.Discord.Services.Extensions;

public static class StringExtension
{

    private static readonly Regex s_regex;

    static StringExtension()
    {
        // Group 1 (Type) - optional - either ! or &
        // Group 2 (Id) - optional - 17-20 digits long userID
        // Match - Either literal @everyone or @here, alternatively <@12345678...9> with optional ! or & behind @
        s_regex = new Regex(@"(?:@(?:everyone|here))|(?:<@([!&])?(\d{17,20})>)");
    }

    /// <summary>
    /// Sanitizes all Discord mentions, except channel ones. If <paramref name="guild"/> is provided,
    /// mentioned users and roles are transcribed to their corresponding plain name. Dangerous names are sanitized too.
    /// </summary>
    /// <param name="value">String to process.</param>
    /// <param name="guild">Discord guild to search plain name of the mentioned user or role.</param>
    /// <returns>Sanitized string of Discord mentions.</returns>
    public static string RemoveDiscordMentions(
        this string value,
        DiscordGuild? guild = null)
    {
        return s_regex.Replace(value, match => MentionEvaluateAsync(match, guild).Result);
    }

    private static async Task<string> MentionEvaluateAsync(Match match, DiscordGuild? guild)
    {
        // Invalidate ID pings, replacing them with their name (only in guilds)
        if (guild is null || !match.Groups.TryGetValue("2", out Group? idMention) || idMention.Value == "")
        {
            return match.Value.Replace("@",
                "@" + char.ConvertFromUtf32(int.Parse("200b", System.Globalization.NumberStyles.HexNumber)));
        }

        ulong snowflakeId = ulong.Parse(idMention.Value);

        if (match.Groups.TryGetValue("1", out Group? idType) && idType.Value == "&")
        {
            DiscordRole mentionedRole = guild.GetRole(snowflakeId);
            if (mentionedRole is not null)
                return mentionedRole.Name.RemoveDiscordMentions();
        }
        else
        {
            try
            {
                DiscordMember mentionedMember = await guild.GetMemberAsync(snowflakeId);
                return mentionedMember.DisplayName.RemoveDiscordMentions();
            }
            catch (NotFoundException)
            {}
        }

        // Invalidate @everyone, @here, or pings which have correct ID format, but no name was found for them
        return match.Value.Replace("@",
            "@" + char.ConvertFromUtf32(int.Parse("200b", System.Globalization.NumberStyles.HexNumber)));
    }
}
