using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Services.Commands.Polls
{
    public class YesNoPoll : IPoll
    {
        private readonly string _authorUsername;
        private readonly string _authorAvatarUrl;
        private readonly string _question;

        public YesNoPoll(string authorUsername, string authorAvatarUrl, string question)
        {
            _authorUsername = authorUsername;
            _authorAvatarUrl = authorAvatarUrl;
            _question = question;
        }

        public async Task Post(DiscordClient client, DiscordChannel channel)
        {
            DiscordMessage pollMessage = await client.SendMessageAsync(channel, embed: Build());

            var _ = Task.Run(async () => { await AddReactions(client, pollMessage); });
        }

        private async Task AddReactions(DiscordClient client, DiscordMessage message)
        {
            await message.CreateReactionAsync(DiscordEmoji.FromName(client, ":thumbsup:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(client, ":thumbsdown:"));
        }

        private DiscordEmbed Build()
        {
            return new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor {Name = _authorUsername, IconUrl = _authorAvatarUrl},
                Title = _question
            }.Build();
        }
    }
}
