using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Services.Commands.Polls
{
    public class AbcPoll : IPoll
    {
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

        private readonly string _authorUsername;
        private readonly string _authorAvatarUrl;
        private readonly string _question;
        private readonly List<string> _answers;

        public AbcPoll(string authorUsername, string authorAvatarUrl, string question, List<string> answers)
        {
            _authorUsername = authorUsername;
            _authorAvatarUrl = authorAvatarUrl;
            _question = question;
            _answers = answers;
        }

        public async Task Post(DiscordClient client, DiscordChannel channel)
        {
            var pollMessage = await client.SendMessageAsync(channel, embed: Build(client));
            await AddReactions(client, pollMessage);
        }

        private async Task AddReactions(DiscordClient client, DiscordMessage message)
        {
            foreach (var emoji in _optionsEmoji.Take(_answers.Count))
            {
                await message.CreateReactionAsync(DiscordEmoji.FromName(client, emoji));
            }
        }

        private DiscordEmbed Build(DiscordClient client)
        {
            var builder = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor {Name = _authorUsername, IconUrl = _authorAvatarUrl},
                Title = _question
            };

            _answers.Zip(_optionsEmoji).ToList().ForEach(pair =>
            {
                var (answer, emojiName) = pair;

                builder.AddField(
                    DiscordEmoji.FromName(client, emojiName).ToString(),
                    answer,
                    true);
            });

            return builder.Build();
        }
    }
}
