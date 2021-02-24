using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Extensions;

namespace HonzaBotner.Discord.Services.Commands.Polls
{
    public class AbcPoll : IPoll
    {
        public static int MaxOptions => _optionsEmoji.Count;

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
            ":regional_indicator_p:"
        };

        private readonly string _authorUsername;
        private readonly string _authorAvatarUrl;
        private readonly string _question;
        private readonly List<string> _options;

        public AbcPoll(string authorUsername, string authorAvatarUrl, string question, List<string> options)
        {
            _authorUsername = authorUsername;
            _authorAvatarUrl = authorAvatarUrl;
            _question = question;
            _options = options;
        }

        public async Task PostAsync(DiscordClient client, DiscordChannel channel)
        {
            DiscordMessage pollMessage = await client.SendMessageAsync(channel, embed: Build(client, channel.Guild));

            Task _ = Task.Run(async () => { await AddReactions(client, pollMessage); });
        }

        private async Task AddReactions(DiscordClient client, DiscordMessage message)
        {
            foreach (string emoji in _optionsEmoji.Take(_options.Count))
            {
                await message.CreateReactionAsync(DiscordEmoji.FromName(client, emoji));
            }
        }

        private DiscordEmbed Build(DiscordClient client, DiscordGuild guild)
        {
            DiscordEmbedBuilder builder = new()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor {Name = _authorUsername, IconUrl = _authorAvatarUrl},
                Title = _question.RemoveDiscordMentions(guild)
            };

            if (_options.Count > _optionsEmoji.Count)
            {
                throw new ArgumentException("Too many options.");
            }

            _options.Zip(_optionsEmoji).ToList().ForEach(pair =>
            {
                (string? answer, string? emojiName) = pair;

                builder.AddField(
                    DiscordEmoji.FromName(client, emojiName).ToString(),
                    answer.RemoveDiscordMentions(guild),
                    true);
            });

            return builder.Build();
        }
    }
}
