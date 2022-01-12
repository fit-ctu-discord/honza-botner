using System.Threading.Tasks;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Extensions;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Managers
{
    public class ReminderManager : IReminderManager
    {
        private readonly IGuildProvider _guildProvider;
        private readonly ILogger<ReminderManager> _logger;

        public ReminderManager(IGuildProvider guildProvider, ILogger<ReminderManager> logger)
        {
            _guildProvider = guildProvider;
            _logger = logger;
        }

        public async Task<DiscordEmbed> CreateDmReminderEmbedAsync(Reminder reminder)
        {
            var guild = await _guildProvider.GetCurrentGuildAsync();

            return await _CreateReminderEmbed(
                $"🔔 Reminder from {guild.Name} Discord",
                reminder,
                DiscordColor.Yellow,
                true
            );
        }

        public async Task<DiscordEmbed> CreateReminderEmbedAsync(Reminder reminder)
        {
            return await _CreateReminderEmbed(
                "🔔 Reminder",
                reminder,
                DiscordColor.Yellow,
                true
            );
        }

        public async Task<DiscordEmbed> CreateExpiredReminderEmbedAsync(Reminder reminder)
        {
            return await _CreateReminderEmbed(
                "🔕 Expired reminder",
                reminder,
                DiscordColor.Grayple
            );
        }

        public async Task<DiscordEmbed> CreateCanceledReminderEmbedAsync(Reminder reminder)
        {
            return await _CreateReminderEmbed(
                "🛑 Canceled reminder",
                reminder,
                DiscordColor.Red
            );
        }

        private async Task<DiscordEmbed> _CreateReminderEmbed(
            string title,
            Reminder reminder,
            DiscordColor color,
            bool useDateTime = false
        )
        {
            var guild = await _guildProvider.GetCurrentGuildAsync();

            DiscordMember? author = await guild.GetMemberAsync(reminder.OwnerId);

            var embedBuilder = new DiscordEmbedBuilder()
                .WithTitle(title)
                .WithAuthor(
                    author?.DisplayName ?? "Unknown user",
                    iconUrl: author?.AvatarUrl)
                .WithDescription(reminder.Content.RemoveDiscordMentions(guild, _logger))
                .WithColor(color);

            if (useDateTime)
            {
                embedBuilder.WithTimestamp(reminder.DateTime);
            }

            return embedBuilder.Build();
        }
    }
}
