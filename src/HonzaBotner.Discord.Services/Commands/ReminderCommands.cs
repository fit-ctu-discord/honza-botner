using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

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
            [Description("Date or time of the reminder")] string date,
            [Description("Optional additional content"), RemainingText] string content
        )
        {
            var emoji = DiscordEmoji.FromName(context.Client, ":ok_hand:");
            await context.Message.CreateReactionAsync(emoji);
        }
    }
}
