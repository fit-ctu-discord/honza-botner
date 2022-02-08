using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Services.Commands.Polls
{
    public class YesNoPoll : Poll
    {
        public override string PollType => "YesNoPoll";
        public override List<string> OptionsEmoji => new() { ":+1:", ":-1:" };
        public override List<string> ActiveEmojis => OptionsEmoji;

        public YesNoPoll(string authorMention, string question) : base(authorMention, question)
        {
        }

        public YesNoPoll(DiscordMessage message) : base(message)
        {
        }

        public override Task AddOptionsAsync(DiscordClient client, IEnumerable<string> newOptions) =>
            throw new ArgumentException($"Adding options is disabled for {PollType}");
    }
}
