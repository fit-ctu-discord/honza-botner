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
        get => OptionsEmoji.GetRange(0, Choices.Count);
    }

    public readonly List<string> Choices;
    private string? _authorMention;
    private readonly string _question;
    private readonly DiscordMessage? _existingPollMessage;


    protected Poll(string question, List<string>? options = null)
    {
        _question = question;
        Choices = options ?? new List<string>();
    }

    protected Poll(DiscordMessage originalMessage)
    {
        _existingPollMessage = originalMessage;
        DiscordEmbed originalPoll = _existingPollMessage.Embeds[0];

        Choices = originalPoll.Fields?
            .Select(ef => ef.Value)
            .ToList() ?? new List<string>();

        _question = originalPoll.Title;
    }

    public async Task PostAsync(DiscordClient client, DiscordChannel channel, string authorMention)
    {
        _authorMention = authorMention;
        DiscordMessage pollMessage = await client.SendMessageAsync(channel, Build(client, channel.Guild));

        Task _ = Task.Run(async () => { await AddReactionsAsync(client, pollMessage); });
    }

    public virtual async Task AddOptionsAsync(DiscordClient client, string authorMention, List<string> newOptions)
    {
        _authorMention = authorMention;
        if (_existingPollMessage == null || Choices == null)
        {
            throw new InvalidOperationException("You can not edit this poll.");
        }
        if (newOptions.Count == 0 || newOptions.Count + Choices.Count > OptionsEmoji.Count)
        {
            throw new ArgumentException($"Invalid amount of options. Don't cross {OptionsEmoji.Count}");
        }

        Choices.AddRange(newOptions);

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

    protected DiscordEmbed Build(DiscordClient client, DiscordGuild guild)
    {
        if (Choices.Count > OptionsEmoji.Count)
        {
            throw new ArgumentException($"Too many options. Maximum options is {OptionsEmoji.Count}.");
        }

        DiscordEmbedBuilder builder = new()
        {
            Title = _question.RemoveDiscordMentions(guild),
            Description = "By: " + _authorMention
        };

        Choices.Zip(ActiveEmojis).ToList().ForEach(pair =>
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
