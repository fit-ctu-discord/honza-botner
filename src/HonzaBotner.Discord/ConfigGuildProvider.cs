using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord;

public class ConfigGuildProvider : IGuildProvider
{
    public ulong GuildId { get; }
    private readonly DiscordClient _client;

    public ConfigGuildProvider(DiscordWrapper wrapper, IOptions<DiscordConfig> config)
    {
        if (config.Value.GuildId is null)
        {
            throw new InvalidOperationException("GuildId not configured");
        }
        GuildId = config.Value.GuildId ?? 0;
        _client = wrapper.Client;
    }

    public Task<DiscordGuild> GetCurrentGuildAsync()
    {
        return _client.GetGuildAsync(GuildId);
    }
}
