using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace HonzaBotner.Discord.Extensions
{
    public static class DiscordMemberExtensions
    {
        public static string RatherNicknameThanUsername(this DiscordMember member)
        {
            return member.Nickname ?? member.Username;
        }

        public static IEnumerable<Page> GeneratePages(this InteractivityExtension _, string input,
            DiscordEmbedBuilder? embedBase = null, int pageRows = 14)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("You must provide a string that is not null or empty!");

            DiscordEmbedBuilder embed = embedBase ?? new DiscordEmbedBuilder();

            List<Page> result = new();

            string[] subSplit = input.Split('\n');

            List<string> split = new();
            string temp = "";

            for (int i = 0; i < subSplit.Length; i++)
            {
                temp += $"{subSplit[i]}\n";
                if (i % pageRows == 0 && i != 0)
                {
                    split.Add(temp);
                    temp = "";
                }
            }

            if (split.All(x => x != temp))
                split.Add(temp);

            int page = 1;
            foreach (string s in split)
            {
                result.Add(new Page("",
                    new DiscordEmbedBuilder(embed).WithDescription(s).WithFooter($"Page {page}/{split.Count}")));
                page++;
            }

            return result;
        }
    }
}
