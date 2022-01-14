using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers;

public class MemeChannelMessageHandler : IEventHandler<MessageCreateEventArgs>
{

    private readonly ulong[] _memeChannels;
    private readonly DiscordClient _client;

    public MemeChannelMessageHandler(IOptions<CommonCommandOptions> options, DiscordWrapper wrapper)
    {
        _memeChannels = options.Value.MemeChannels ?? Array.Empty<ulong>();
        _client = wrapper.Client;
    }

    public async Task<EventHandlerResult> Handle(MessageCreateEventArgs args)
    {
        if (!args.Author.IsBot
            && _memeChannels.Contains(args.Channel.Id)
            && !args.Message.Attachments.Any()
            && !args.Message.Content.Contains("https://"))
            await args.Message.CreateReactionAsync(DiscordEmoji.FromName(_client, ":thread:"));

        return EventHandlerResult.Continue;
    }

}
