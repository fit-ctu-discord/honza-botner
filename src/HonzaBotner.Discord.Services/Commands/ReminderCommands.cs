using System;
using System.Globalization;
using System.Threading.Tasks;
using Chronic.Core;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Extensions;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Commands;

[Group("reminder")]
[Aliases("remind")]
[Description("Create a new reminder using subcommand `create` " +
             "and formats like: `03.03.2002 01:05:16`, `07:22:16`, `in 2 hours`, ...")]
[ModuleLifespan(ModuleLifespan.Transient)]
[RequireGuild]
public class ReminderCommands : BaseCommandModule
{
    private readonly IRemindersService _service;

    private readonly ReminderOptions _options;

    private readonly IReminderManager _reminderManager;

    public ReminderCommands(
        IRemindersService service,
        IOptions<ReminderOptions> options,
        IReminderManager reminderManager
    )
    {
        _service = service;
        _options = options.Value;
        _reminderManager = reminderManager;
    }

    [GroupCommand]
    [Command("help")]
    [Description("Get help about reminder creation")]
    public async Task HelpAsync(CommandContext ctx)
    {
        var helpEmbed = new DiscordEmbedBuilder()
            .WithTitle("Creating reminders")
            .WithDescription(ctx.Command.Parent?.Description ?? ctx.Command.Description)
            .WithColor(DiscordColor.Blue)
            .AddField(
                "Usage:",
                "`reminder create \"<rawDatetime>\" [content...]`" +
                "\n`remind me \"in 10 years\" fix that small botner bug`" +
                "\n`reminder create \"1.9. 12:00\" Welcome new first-graders`"
            )
            .AddField(
                "Cooldown",
                "Due to database limitations, everyone is capped at creating max 2 reminders per hour")
            .Build();
        var message = new DiscordMessageBuilder()
            .AddEmbed(helpEmbed)
            .AddComponents(
                new DiscordLinkButtonComponent(
                    "https://docs.microsoft.com/en-us/dotnet/api/system.datetime.parse#the-string-to-parse",
                    "More datetime formats"));

        await ctx.RespondAsync(message);
    }

    [Command("create")]
    [Aliases("me")] // Allows a more "fluent" usage ::remind me <>
    [Description("Create a new reminder using formats like: `03.03.2002 01:05:16`, `07:22:16`, `in 2 hours`, ..." +
                 "\nUsage: `remind me \"*when*\" *what about*`")]
    [Cooldown(2, 60 * 60, CooldownBucketType.User)]
    public async Task CreateAsync(
        CommandContext context,
        [Description("Date or time of the reminder")]
        string rawDatetime,
        [Description("Content of the reminder."), RemainingText]
        string? content
    )
    {
        DateTime now = DateTime.UtcNow;
        DateTime? datetime = ParseDateTime(rawDatetime);

        if (content == null)
        {
            await context.RespondErrorAsync(
                "Cannot parse content string",
                "You didn't provide any content for the reminder."
            );
            await context.Message.DeleteAsync();
            return;
        }

        if (datetime == null)
        {
            await context.RespondErrorAsync(
                $"Cannot parse datetime string `{rawDatetime}`",
                "Try using an explicit datetime or expressions like:" +
                "`in 30 minutes`, `tomorrow at 8:00`, `18.08.2021 07:22:16`, ..."
            );
            return;
        }

        if (datetime <= now)
        {
            await context.RespondErrorAsync(
                "Cannot schedule reminders in the past.",
                "You can only create reminders that are in the future."
            );
            return;
        }

        DiscordMessage message = await context.Channel.SendMessageAsync("Creating reminder...");

        Reminder reminder = await _service.CreateReminderAsync(
            context.User.Id,
            message.Id,
            message.ChannelId,
            datetime.Value.ToUniversalTime(), // This is safe, as the nullability is validated above
            content
        );

        await message.ModifyAsync("", await _reminderManager.CreateReminderEmbedAsync(reminder));
        await message.CreateReactionAsync(DiscordEmoji.FromUnicode(_options.CancelEmojiName));
        await message.CreateReactionAsync(DiscordEmoji.FromUnicode(_options.JoinEmojiName));
        await context.Message.DeleteAsync();
    }

    private static DateTime? ParseDateTime(string datetime)
    {
        if (DateTime.TryParse(datetime, new CultureInfo("cs-CZ"), DateTimeStyles.AllowWhiteSpaces,
                out DateTime parsed))
        {
            return parsed;
        }

        return new Parser().Parse(datetime)?.Start;
    }
}
