using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Extensions
{
    public static class StringExtension
    {
        private static readonly List<string> _special = new() {"everyone", "here"};

        public static string RemoveDiscordMentions(this string? stringObject, DiscordGuild guild,
            ILogger? logger = null)
        {
            string value = stringObject ?? "";

            foreach (string s in _special)
            {
                value = Regex.Replace(value ?? "", @$"@{s}", s);
            }

            value = Regex.Replace(value ?? "", @"<@(.)(.*?)>", match =>
                {
                    if (!match.Groups.TryGetValue("1", out Group? mentionTypeGroup)) return "";
                    if (!match.Groups.TryGetValue("2", out Group? idValueGroup)) return "";

                    string idString = idValueGroup.Value;
                    if (!ulong.TryParse(idString, out ulong id)) return "";

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
                                // This is not a mention.
                                return match.Value;
                        }
                    }
                    catch (Exception e)
                    {
                        logger?.LogWarning(e, "Removing mention failed.");
                        return "";
                    }
                }
            );

            return value;
        }
    }
}
