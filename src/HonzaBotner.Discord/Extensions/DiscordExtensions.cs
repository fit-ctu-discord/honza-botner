using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace HonzaBotner.Discord.Extensions
{
    public static class DiscordExtensions
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

        public static IEnumerable<Page> GeneratePages(this InteractivityExtension _,
            List<(string, string)> items,
            DiscordEmbedBuilder? embedBase = null, int pageRows = 14)
        {
            int pagesCount = items.Count / pageRows + (items.Count % pageRows > 0 ? 1 : 0);
            DiscordEmbedBuilder embed = embedBase ?? new DiscordEmbedBuilder();
            List<Page> result = new();

            int item = 0;
            int page = 0;
            foreach ((string name, string description) in items)
            {
                string descriptionValue = description;
                if (description.Length > 250)
                {
                    descriptionValue = description.Substring(0, 250) + "...";
                }

                embed.AddField(name, descriptionValue);
                item++;

                if (item == pageRows || page * pageRows + item == items.Count)
                {
                    item = 0;
                    page++;
                    result.Add(
                        new Page("",
                            new DiscordEmbedBuilder(embed).WithFooter($"Page {page}/{pagesCount}")
                        )
                    );
                    embed.ClearFields();
                }
            }

            return result;
        }
    }
}
