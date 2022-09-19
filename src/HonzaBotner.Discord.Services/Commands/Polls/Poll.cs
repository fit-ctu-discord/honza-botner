using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Extensions;

namespace HonzaBotner.Discord.Services.Commands.Polls;

public abstract class Poll
{
    protected abstract List<string> OptionsEmoji { get; }
    public abstract string PollType { get; }
    protected virtual List<string> UsedEmojis
    {
        get => OptionsEmoji.GetRange(0, NewChoices.Count);
    }

    protected List<string> NewChoices;

    public readonly string AuthorMention;
    protected readonly DiscordMessage? ExistingPollMessage;
    protected readonly string Question;

    protected Poll(string authorMention, string question, List<string>? options = null)
    {
        AuthorMention = authorMention;
        Question = question;
        NewChoices = options ?? new List<string>();
    }

    protected Poll(DiscordMessage originalMessage)
    {
        ExistingPollMessage = originalMessage;
        DiscordEmbed originalPoll = ExistingPollMessage.Embeds[0];
        int startIndex = originalPoll.Description.LastIndexOf("<", StringComparison.Ordinal);

        // Extract original author Mention via discord's mention format <@!123456789>.
        AuthorMention = originalPoll.Description[startIndex..];

        Question = originalPoll.Title;
        NewChoices = new List<string>();
    }

    public async Task PostAsync(DiscordClient client, DiscordChannel channel)
    {
        DiscordMessage pollMessage = await client.SendMessageAsync(channel, Build(client, channel.Guild));

        Task _ = Task.Run(async () => { await AddReactionsAsync(client, pollMessage); });
    }

    protected async Task AddReactionsAsync(DiscordClient client, DiscordMessage message, List<string>? reactions = null)
    {
        foreach (string reaction in reactions ?? UsedEmojis)
        {
            await message.CreateReactionAsync(DiscordEmoji.FromName(client, reaction));
        }
    }

    private DiscordEmbed Build(DiscordClient client, DiscordGuild guild)
    {
        if (NewChoices.Count > OptionsEmoji.Count)
        {
            throw new PollException($"Too many options. Maximum options is {OptionsEmoji.Count}.");
        }

        DiscordEmbedBuilder builder = new()
        {
            Title = Question.RemoveDiscordMentions(guild),
            Description = "By: " + AuthorMention // Author needs to stay as the last argument
        };

        NewChoices.Zip(UsedEmojis).ToList().ForEach(pair =>
        {
            (string? answer, string? emojiName) = pair;

            builder.AddField(
                DiscordEmoji.FromName(client, emojiName).ToString(),
                answer.RemoveDiscordMentions(guild),
                true);
        });

        return builder.WithFooter(PollType).Build();
    }
}
