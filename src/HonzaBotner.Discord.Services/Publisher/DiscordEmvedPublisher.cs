using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Publisher;

public class DiscordEmbedPublisher : IPublisherService
{
    private readonly ILogger<DiscordEmbedPublisher> _logger;
    private readonly IGuildProvider _guildProvider;

    public DiscordEmbedPublisher(ILogger<DiscordEmbedPublisher> logger, IGuildProvider guildProvider)
    {
        _logger = logger;
        _guildProvider = guildProvider;
    }

    private static string Limit(string str, int limit)
    {
        const string andMore = "...";

        // Just to be safe
        if (str.Length < limit)
        {
            return str;
        }

        StringBuilder sb = new(str.Substring(0, limit - 1 - andMore.Length));
        sb.Append(andMore);

        return sb.ToString();
    }

    public async Task Publish(News news, params ulong[] channels)
    {
        DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();

        DiscordEmbedBuilder builder = new()
        {
            // Embed titles are limited to 256 characters
            Title = Limit(news.Title, 256),
            Author = new DiscordEmbedBuilder.EmbedAuthor()
            {
                // The author name is limited to 256 characters
                Name = Limit(news.Author, 256)
                // If the Url is email (mailto:), it wont work :(
                //, Url = news.AuthorLink
            },
            // Embed descriptions are limited to 4096 characters
            Description = Limit(news.Content, 4096),
            Url = news.Link,
            Timestamp = news.CreatedAt,
            Color = Optional.FromValue(DiscordColor.Blurple)
        };

        DiscordEmbed embed = builder.Build();

        foreach (ulong channelId in channels)
        {
            if (!guild.Channels.TryGetValue(channelId, out DiscordChannel? channel) || channel == null)
            {
                _logger.LogWarning("Couldn't find channel with id {ChannelId} for publishing news",
                    channelId);
            }

            await channel!.SendMessageAsync(embed);
        }
    }
}
