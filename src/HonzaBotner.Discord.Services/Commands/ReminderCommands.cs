using System;
using System.Globalization;
using System.Threading.Tasks;
using Chronic.Core;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Database;
using HonzaBotner.Discord.Extensions;
using HonzaBotner.Services.Contract;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("reminder")]
    [Aliases("remind")]
    [Description("Commands to manager reminders.")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class ReminderCommands : BaseCommandModule
    {
        private readonly IRemindersService _service;

        public ReminderCommands(IRemindersService service)
        {
            _service = service;
        }

        [Command("create")]
        [Aliases("me")] // Allows a more "fluent" usage ::remind me <>
        [Description("Create a new reminder.")]
        public async Task Create(
            CommandContext context,
            [Description("Title of the reminder.")] string title,
            [Description("Date or time of the reminder")] string rawDatetime,
            [Description("Optional additional content"), RemainingText] string content
        )
        {
            var datetime = ParseDateTime(rawDatetime);

            if (datetime == null)
            {
                await context.RespondErrorAsync(
                    $"Cannot parse datetime string `{rawDatetime}`",
                    "Try using an explicit datetime or expressions like `in 30 minutes`, `tomorrow at 8:00`, ..."
                );
                return;
            }

            var message = await context.RespondAsync("Creating reminder...");

            var reminder = await _service.CreateReminderAsync(message.Id);

            await message.ModifyAsync(
                "",
                CreateReminderEmbed(context, reminder)
            );
        }

        private static DateTime? ParseDateTime(string datetime)
        {
            // First try to parse the explicit datetime formats
            // Cases with time only are handled by the parser
            string[] formats = { "dd. MM. yyyy HH:mm", "dd.MM.yyyy HH:mm", "dd. MM. yyyy", "dd.MM.yyyy" };

            if (DateTime.TryParseExact(datetime, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime parsed))
            {
                return parsed;
            }

            return new Parser().Parse(datetime)?.Start;
        }

        private static DiscordEmbed CreateReminderEmbed(CommandContext context, Reminder reminder)
        {
            return new DiscordEmbedBuilder()
                .WithAuthor(context.Member.RatherNicknameThanUsername(), null, context.Member.AvatarUrl)
                .WithColor(DiscordColor.Blurple)
                .WithTitle("Reminder created.")
                .WithDescription(
@"If you would like to cancel this reminder, click the . reaction.
For others: if you would like to be notified with this reminder as well, click the . reaction."
                )
                .AddField("Title", reminder.Title.RemoveDiscordMentions(context.Guild))
                .AddField("Content", reminder.Content?.RemoveDiscordMentions(context.Guild) ?? "No content provided")
                .AddField("Date / time of the reminder", reminder.DateTime.ToString(CultureInfo.InvariantCulture))
                .Build();
        }
    }
}
