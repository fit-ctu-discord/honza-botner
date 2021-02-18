using System;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Extensions
{
    public static class StringExtension
    {
        public static string RemoveDiscordMentions(this string? stringObject, DiscordGuild guild)
        {
            return Regex.Replace(stringObject ?? "", @"<@!(.*?)>", match =>
            {
                Group? idValue;
                if (!match.Groups.TryGetValue("1", out idValue)) return "";

                string userIdString = idValue.Value;
                ulong userId;
                if (!ulong.TryParse(userIdString, out userId)) return "";

                try
                {
                    // Make this better async? https://stackoverflow.com/questions/33014011/how-to-rewrite-regex-replace-due-to-async-api
                    DiscordMember member = guild.GetMemberAsync(userId).Result;
                    return member.RatherNicknameThanUsername();
                }
                catch (Exception e)
                {
                    // TODO: how to use _logger?
                    return "";
                }
            });
        }
    }
}
