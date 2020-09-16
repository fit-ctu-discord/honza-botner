using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Command;

namespace HonzaBotner.Commands.Pools
{
    public class Abc : IChatCommand
    {
        public const string ChatCommand = "abc";
        // ;abc <question> <option 1> <option 2> ...
        // ;abc "<question>" "<option 1 with spaces>" "<option 2 with spaces>" ...

        private static readonly List<string> _optionsEmoji = new List<string>
        {
            ":regional_indicator_a:",
            ":regional_indicator_b:",
            ":regional_indicator_c:",
            ":regional_indicator_d:",
            ":regional_indicator_e:",
            ":regional_indicator_f:",
            ":regional_indicator_g:",
            ":regional_indicator_h:",
            ":regional_indicator_i:",
            ":regional_indicator_j:",
            ":regional_indicator_k:",
            ":regional_indicator_l:",
            ":regional_indicator_m:",
            ":regional_indicator_n:",
            ":regional_indicator_o:",
            ":regional_indicator_p:",
            ":regional_indicator_q:",
            ":regional_indicator_r:",
            ":regional_indicator_s:",
            ":regional_indicator_t:"
        };

        public async Task<ChatCommendExecutedResult> ExecuteAsync(DiscordClient client, DiscordMessage message,
            CancellationToken cancellationToken)
        {
            if (message.Author.IsBot) return ChatCommendExecutedResult.CannotBeUsedByBot;
            if (message.Content.Split(" ").Length < 3) return ChatCommendExecutedResult.WrongSyntax;

            const string commandPattern = @"^.\w+\s+";
            string text = Regex.Replace(message.Content, commandPattern, "");

            const string tokenPattern = @"""([^""]+)""|[\S]+";
            MatchCollection matches = Regex.Matches(text, tokenPattern);
            List<string> arguments = matches.Cast<Match>()
                .Select(match => match.Groups[1].Value != "" ? match.Groups[1].Value : match.Value).ToList();

            if (arguments.Count == 0) return ChatCommendExecutedResult.WrongSyntax;

            string question = arguments.First();
            string authorNickName = message.Channel.Guild.GetMemberAsync(message.Author.Id).Result.Nickname;

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = authorNickName, IconUrl = message.Author.AvatarUrl
                },
                Title = question,
            };

            int optionIndex = 0;
            string emoji = _optionsEmoji[optionIndex];

            foreach (var answer in arguments.Skip(1))
            {
                if (answer.Trim() == "") return ChatCommendExecutedResult.WrongSyntax;

                embed.AddField(
                    DiscordEmoji.FromName(client, _optionsEmoji[optionIndex]).ToString(),
                    answer,
                    true
                );

                optionIndex++;
            }

            try
            {
                var poolMessage = await client.SendMessageAsync(message.Channel, embed: embed.Build());
                await message.DeleteAsync();

                for (int i = 0; i < optionIndex; i++)
                {
                    await poolMessage.CreateReactionAsync(DiscordEmoji.FromName(client, _optionsEmoji[i]));
                }
            }
            catch
            {
                return ChatCommendExecutedResult.InternalError;
            }

            return ChatCommendExecutedResult.Ok;
        }
    }
}
