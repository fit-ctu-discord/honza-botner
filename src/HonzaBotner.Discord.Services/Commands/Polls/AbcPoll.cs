using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Extensions;

namespace HonzaBotner.Discord.Services.Commands.Polls;

public class AbcPoll : Poll
{
    public override string PollType => "AbcPoll";

    protected override List<string> OptionsEmoji => new()
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

    public AbcPoll(string authorMention, string question, List<string> options)
        : base(authorMention, question, options)
    {
    }

    public AbcPoll(DiscordMessage message) : base(message)
    {
    }

    public async Task AddOptionsAsync(DiscordClient client, IEnumerable<string> newOptions)
    {
        if (ExistingPollMessage is null)
        {
            throw new InvalidOperationException("You can edit only poll constructed from sent message.");
        }

        NewChoices = newOptions.ToList();

        if (NewChoices.Count + ExistingPollMessage.Embeds[0].Fields.Count > OptionsEmoji.Count)
        {
            throw new PollException($"Too many options. Maximum options is {OptionsEmoji.Count}.");
        }

        List<string> emojisToAdd = OptionsEmoji;
        emojisToAdd.RemoveAll(emoji =>
            ExistingPollMessage.Reactions.Select(rect => rect.Emoji).Contains(DiscordEmoji.FromName(client, emoji)));

        emojisToAdd = emojisToAdd.GetRange(
            0, Math.Min(
                Math.Min(
                    emojisToAdd.Count,
                    20 - (ExistingPollMessage.Reactions.Count <= 20 ? ExistingPollMessage.Reactions.Count : 20)),
                NewChoices.Count));
        NewChoices = NewChoices.GetRange(0, emojisToAdd.Count);



        await ExistingPollMessage
            .ModifyAsync(Modify(client, ExistingPollMessage.Channel.Guild, ExistingPollMessage.Embeds[0], emojisToAdd));

        Task _ = Task.Run(async () => { await AddReactionsAsync(client, ExistingPollMessage, emojisToAdd); });
    }

    private DiscordEmbed Modify(DiscordClient client, DiscordGuild guild, DiscordEmbed original, List<string> emojisToAdd)
    {
        DiscordEmbedBuilder builder = new (original);

        NewChoices.Zip(emojisToAdd).ToList().ForEach(pair =>
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
