using System;
using System.Globalization;
using System.Threading.Tasks;
using Chronic.Core;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Discord.Services.Extensions;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Commands;

[SlashCommandGroup("reminder", "Commands to manage reminders.")]
[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public class ReminderCommands : ApplicationCommandModule
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

    [SlashCommand("create", "Create reminder of your choice")]
    public async Task CreateCommandAsync(
        InteractionContext ctx,
        [Option("when", "26th April, in 2 days, tomorrow 3:00, ...")] string rawDatetime,
        [Option("content", "What to be reminded about.")] string content
    )
    {
        DateTime now = DateTime.UtcNow;
        DateTime? datetime = ParseDateTime(rawDatetime);

        if (datetime is null)
        {
            await ctx.CreateResponseAsync(
                $"Cannot parse datetime string `{rawDatetime}`" +
                "Try using an explicit datetime or expressions like:" +
                "`in 30 minutes`, `tomorrow at 8:00`, `18.08.2021 07:22:16`, ...", true);
            return;
        }

        if (datetime <= now)
        {
            await ctx.CreateResponseAsync(
                "It seems like the reminder you are trying to set up is scheduled in past.\n" +
                "It might be mistake of our parser, can you specify date more precisely?", true);
            return;
        }

        DiscordMessage followup = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("❤"));

        Reminder reminder = await _service.CreateReminderAsync(
            ctx.User.Id,
            followup.Id,
            followup.ChannelId,
            datetime.Value.ToUniversalTime(), // This is safe, as the nullability is validated above
            content.RemoveDiscordMentions(ctx.Guild)
        );

        followup = await ctx.EditFollowupAsync(followup.Id,
            new DiscordWebhookBuilder().AddEmbed(await _reminderManager.CreateReminderEmbedAsync(reminder)));
        await followup.CreateReactionAsync(DiscordEmoji.FromUnicode(_options.CancelEmojiName));
        await followup.CreateReactionAsync(DiscordEmoji.FromUnicode(_options.JoinEmojiName));
        await ctx.CreateResponseAsync("Reminder created", true);
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
