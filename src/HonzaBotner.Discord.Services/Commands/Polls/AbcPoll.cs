using System.Collections.Generic;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Services.Commands.Polls
{
    public class AbcPoll : Poll
    {
        public override string PollType => "AbcPoll";
        public override List<string> OptionsEmoji => new()
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
            : base(authorMention, question, options) {}

        public AbcPoll(DiscordMessage message) : base(message) {}
    }
}
