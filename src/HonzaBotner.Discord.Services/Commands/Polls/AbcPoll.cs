using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

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
        const int reactionCap = 20; // Max amount of reactions present on a message, lower than Discord provided, in case some trolls block bot's reactions
        if (ExistingPollMessage is null)
        {
            throw new InvalidOperationException("You can edit only poll constructed from sent message.");
        }

        NewChoices = newOptions.ToList();

        List<string> emojisToAdd = OptionsEmoji;

        // Look at existing message and allow only emojis which are not yet present on that message.
        emojisToAdd.RemoveAll(emoji =>
            ExistingPollMessage.Reactions.Select(rect => rect.Emoji).Contains(DiscordEmoji.FromName(client, emoji)));

        emojisToAdd = emojisToAdd.GetRange(
            0,
            // Allow only so many reactions, that we don't cross 20 reactions on existing message
            Math.Min(Math.Min(reactionCap - Math.Min(ExistingPollMessage.Reactions.Count, reactionCap),
                    // Take above reaction capacity, and lower it optionally to number of emojis which we are able to react with
                    emojisToAdd.Count),
                // Take the above number and cap it at total new choices we want to add (can be lower or equal to real choices number)
                NewChoices.Count));

        // The new options count will be equal or lower than total options added, based on available emojis
        NewChoices = NewChoices.GetRange(0, emojisToAdd.Count);

        if (NewChoices.Count == 0)
        {
            throw new PollException($"Total number of reactions on a message can't be greater than {reactionCap}");
        }
        await ExistingPollMessage
            .ModifyAsync(new DiscordMessageBuilder()
                .AddEmbed(Modify(client, ExistingPollMessage.Embeds[0], emojisToAdd)));

        Task _ = Task.Run(async () => { await AddReactionsAsync(client, ExistingPollMessage, emojisToAdd); });
    }

    private DiscordEmbed Modify(DiscordClient client, DiscordEmbed original, IEnumerable<string> emojisToAdd)
    {
        DiscordEmbedBuilder builder = new (original);

        NewChoices.Zip(emojisToAdd).ToList().ForEach(pair =>
        {
            (string? answer, string? emojiName) = pair;

            builder.AddField(
                DiscordEmoji.FromName(client, emojiName).ToString(),
                answer,
                true);
        });

        return builder.WithFooter(PollType).Build();
    }
}
