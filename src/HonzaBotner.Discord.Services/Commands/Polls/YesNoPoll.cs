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
        public override List<string> OptionsEmoji => new List<string>() { ":+1:", ":-1:" };
        public override List<string> ActiveEmojis => OptionsEmoji;

        public YesNoPoll(string question) : base(question){}

        public YesNoPoll(DiscordMessage message) : base(message){}

        public override Task AddOptionsAsync(DiscordClient client, string Mention, List<string> newOptions) =>
            throw new NotImplementedException($"You can not edit {PollType}");
    }
}
