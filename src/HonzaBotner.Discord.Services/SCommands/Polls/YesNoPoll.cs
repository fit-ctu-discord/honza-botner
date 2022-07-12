using System.Collections.Generic;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Services.SCommands.Polls;

public class YesNoPoll : Poll
{
    public override string PollType => "YesNoPoll";
    protected override List<string> OptionsEmoji => new() { ":+1:", ":-1:" };

    protected override List<string> UsedEmojis
    {
        get => OptionsEmoji;
    }

    public YesNoPoll(string authorMention, string question) : base(authorMention, question)
    {
    }

    public YesNoPoll(DiscordMessage message) : base(message)
    {
    }
}
