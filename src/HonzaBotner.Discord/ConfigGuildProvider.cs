using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord
{
    public class ConfigGuildProvider : IGuildProvider
    {
        private readonly IOptions<DiscordConfig> _config;
        private readonly DiscordClient _client;

        public ConfigGuildProvider(DiscordWrapper wrapper, IOptions<DiscordConfig> config)
        {
            _config = config;
            _client = wrapper.Client;
        }

        public Task<DiscordGuild> GetCurrentGuildAsync()
        {
            ulong? guildId = _config.Value?.GuildId;
            if (guildId == null)
            {
                throw new InvalidOperationException("GuildId not configured");
            }

            return _client.GetGuildAsync(guildId.Value);
        }
    }
}
