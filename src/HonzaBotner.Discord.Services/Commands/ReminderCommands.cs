using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Chronic.Core;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Extensions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("reminder")]
    [Aliases("remind")]
    [Description("Commands to manager reminders.")]
    public class ReminderCommands : BaseCommandModule
    {
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

            await context.RespondAsync(datetime.ToString());
        }

        private static DateTime? ParseDateTime(string datetime)
        {
            // First try to parse the explicit datetime formats
            // Cases with time only are handled by the parser
            string[] formats = { "dd. MM. yyyy HH:mm", "dd.MM.yyyy HH:mm", "dd. MM. yyyy", "dd.MM.yyyy" };

            if (DateTime.TryParseExact(datetime, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal,
                out DateTime parsed))
            {
                return parsed;
            }

            return new Parser().Parse(datetime)?.Start;
        }
    }
}
