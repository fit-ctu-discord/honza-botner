using System;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Extensions
{
    public static class StringExtension
    {
        /// <summary>
        /// Sanitizes string of Discord mentions. If method fails, empty string is returned.
        /// </summary>
        /// <param name="stringObject">String to process.</param>
        /// <param name="guild">Discord guild to search name content of the mentioned user or group.</param>
        /// <param name="logger">Logger to log to.</param>
        /// <returns>Sanitized string of Discord mentions.</returns>
        public static string RemoveDiscordMentions(this string stringObject, DiscordGuild guild,
            ILogger? logger = null)
        {
            string value = stringObject;

            // Replace all user and group mentions by their name from.
            value = Regex.Replace(value, @"<@(.)(.*?)>", match =>
                {
                    if (!match.Groups.TryGetValue("1", out Group? mentionTypeGroup)) return "";
                    if (!match.Groups.TryGetValue("2", out Group? idValueGroup)) return "";
                    if (!ulong.TryParse(idValueGroup.Value, out ulong id)) return "";

                    try
                    {
                        // Make this better async? https://stackoverflow.com/questions/33014011/how-to-rewrite-regex-replace-due-to-async-api
                        switch (mentionTypeGroup.Value)
                        {
                            case "!":
                                DiscordMember member = guild.GetMemberAsync(id).Result;
                                return member.RatherNicknameThanUsername();
                            case "&":
                                DiscordRole role = guild.GetRole(id);
                                return role.Name;
                            default:
                                // This is not a mention, no sanitation needed.
                                return match.Value;
                        }
                    }
                    catch (Exception e)
                    {
                        logger?.LogWarning(e, "Removing mention failed");
                        return "";
                    }
                }
            );

            // Replace all remaining @ characters.
            value = Regex.Replace(value, @"@", string.Empty);

            return value;
        }
    }
}
