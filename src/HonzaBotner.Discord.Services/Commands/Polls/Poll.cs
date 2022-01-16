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
    public abstract List<string> OptionsEmoji { get; }
    public abstract string PollType { get; }
    public virtual List<string> ActiveEmojis
    {
        get => OptionsEmoji.GetRange(0, _choices.Count);
    }

    public readonly string AuthorMention;
    private readonly List<string> _choices;
    private readonly DiscordMessage? _existingPollMessage;
    private readonly string _question;


    protected Poll(string authorMention, string question, List<string>? options = null)
    {
        AuthorMention = authorMention;
        _question = question;
        _choices = options ?? new List<string>();

    }

    protected Poll(DiscordMessage originalMessage)
    {
        _existingPollMessage = originalMessage;
        DiscordEmbed originalPoll = _existingPollMessage.Embeds[0];

        // Extract original author Mention via discord's mention format <@!123456789>
        AuthorMention = originalPoll.Description.Substring(
            originalPoll.Description.LastIndexOf("<", StringComparison.Ordinal));

        _choices = originalPoll.Fields?
            .Select(ef => ef.Value)
            .ToList() ?? new List<string>();

        _question = originalPoll.Title;
    }

    public async Task PostAsync(DiscordClient client, DiscordChannel channel)
    {
        DiscordMessage pollMessage = await client.SendMessageAsync(channel, Build(client, channel.Guild));

        Task _ = Task.Run(async () => { await AddReactionsAsync(client, pollMessage); });
    }

    public virtual async Task AddOptionsAsync(DiscordClient client, List<string> newOptions)
    {
        if (_existingPollMessage == null)
        {
            throw new InvalidOperationException("You can edit only poll constructed from sent message.");
        }

        _choices.AddRange(newOptions);

        await _existingPollMessage.ModifyAsync( Build(client, _existingPollMessage.Channel.Guild));

        Task _ = Task.Run(async () => { await AddReactionsAsync(client, _existingPollMessage); });
    }

    protected async Task AddReactionsAsync(DiscordClient client, DiscordMessage message)
    {
        foreach (var reaction in ActiveEmojis)
        {
            await message.CreateReactionAsync(DiscordEmoji.FromName(client, reaction));
        }
    }

    private DiscordEmbed Build(DiscordClient client, DiscordGuild guild)
    {
        if (_choices.Count > OptionsEmoji.Count)
        {
            throw new ArgumentException($"Too many options. Maximum options is {OptionsEmoji.Count}.");
        }

        DiscordEmbedBuilder builder = new()
        {
            Title = _question.RemoveDiscordMentions(guild),
            Description = "By: " + AuthorMention // Author needs to stay as the last argument
        };

        _choices.Zip(ActiveEmojis).ToList().ForEach(pair =>
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
