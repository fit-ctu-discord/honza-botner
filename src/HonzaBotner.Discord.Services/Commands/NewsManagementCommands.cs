using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Attributes;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("news")]
    [Description("Commands to interact with members.")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [RequireMod]
    public class NewsManagementCommands : BaseCommandModule
    {
        private readonly INewsConfigService _configService;

        public NewsManagementCommands(INewsConfigService configService)
        {
            _configService = configService;
        }

        [Command("list")]
        public async Task ListConfig(CommandContext context)
        {
            IList<NewsConfig> configs = await _configService.ListConfigsAsync(false).ConfigureAwait(false);

            DiscordEmbedBuilder builder = new();
            builder.Title = "News List";
            builder.WithTimestamp(DateTime.Now);
            foreach (NewsConfig config in configs)
            {
                string active = GetActiveEmoji(config);
                builder.AddField($"{active} {config.Name} [{config.Id}] ", $"Source: {config.Source}; Publishing to: {string.Join(',', config.Channels)}; Last fetched: {config.LastFetched}");
            }

            await context.Channel.SendMessageAsync(builder.Build());
        }

        private static string GetActiveEmoji(NewsConfig config) => config.Active ? "✔️" : "❌";

        [Command("toogle")]
        [Description("Toggles if one configuration for news source is active or not")]
        public async Task ToggleConfig(CommandContext contex, int id)
        {
            bool currentState = await _configService.ToggleConfig(id);

            string prefix = currentState ? "" : "in";

            await contex.RespondAsync($"News config with id [{id}] set to be {prefix}active");
        }

        [Command("add")]
        public async Task AddConfig(CommandContext context, string name, string source, string newsProviderType, string publisherProviderType, params DiscordChannel[] channels)
        {
            CheckIfTypeExists(newsProviderType, nameof(newsProviderType));
            CheckIfTypeExists(publisherProviderType, nameof(publisherProviderType));

            NewsConfig config = new(default, name, source, DateTime.MinValue, newsProviderType, publisherProviderType, true, channels.Select(ch => ch.Id).ToArray());

            await _configService.AddOrUpdate(config);
        }

        private static void CheckIfTypeExists(string typeName, string paramName)
        {
            _ = Type.GetType(typeName) ?? throw new ArgumentOutOfRangeException(paramName, $"Type `{typeName}` was not found in app domain");
        }
    }
}
