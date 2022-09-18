using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using HonzaBotner.Discord.Services.Extensions;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Commands;

[SlashCommandGroup("moderation", "Punish members of your server or show/edit their history")]
[SlashCommandPermissions(Permissions.BanMembers)]
[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public class ModerationCommands : ApplicationCommandModule
{
    private readonly IWarningService _warningService;
    private readonly ILogger<ModerationCommands> _logger;

    public ModerationCommands(IWarningService warningService, ILogger<ModerationCommands> logger)
    {
        _warningService = warningService;
        _logger = logger;
    }

    [ContextMenu(ApplicationCommandType.UserContextMenu, "warn")]
    public async Task WarnMenuAsync(ContextMenuContext ctx)
    {
        string modalId = $"warn-{ctx.User.Id}-{ctx.TargetUser.Id}";
        string reasonId = "id-reason";

        var response = new DiscordInteractionResponseBuilder()
            .WithTitle($"New Warning for {ctx.TargetMember.DisplayName}")
            .WithCustomId(modalId)
            .AddComponents(new TextInputComponent("Reason:", reasonId, required: true));
        await ctx.CreateResponseAsync(InteractionResponseType.Modal, response);

        var numberOfWarnings = await _warningService.GetNumberOfWarnings(ctx.TargetUser.Id);

        var interactivity = ctx.Client.GetInteractivity();
        var modalReason = await interactivity.WaitForModalAsync(modalId, TimeSpan.FromMinutes(10));
        if (modalReason.TimedOut)
        {
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Response timed out"));
            return;
        }

        int? warningId = await _warningService.AddWarningAsync(ctx.TargetUser.Id, modalReason.Result.Values[reasonId],
            ctx.User.Id);

        if (!warningId.HasValue)
        {
            _logger.LogWarning("Couldn't add a warning for {Member} with {Reason}",
                ctx.TargetMember.DisplayName, modalReason.Result.Values[reasonId]);
            await ctx.FollowUpAsync(
                new DiscordFollowupMessageBuilder().WithContent("Warning was not issued due to error").AsEphemeral());
            return;
        }

        try
        {
            string messageForUser =
                $"You were **warned** on *student FIT Discord server* in channel _#{ctx.Channel.Name}_ (warning id: {warningId})" +
                $"\n\n" +
                $"Reason:\n{modalReason.Result.Values[reasonId]}" +
                "\n\n" +
                $"This is your **{numberOfWarnings + 1}. warning**, #beGood.";

            await ctx.TargetMember.SendMessageAsync(messageForUser.RemoveDiscordMentions(ctx.Guild));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to announce warning");
        }

        await modalReason.Result.Interaction.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .WithContent("Warning issued to " +
                             $"{ctx.TargetMember.Mention} with reason \"{modalReason.Result.Values[reasonId]}\"")
                .AsEphemeral()
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, modalId, "Announce")));

        var buttonResponse = await interactivity.WaitForButtonAsync(
            await modalReason.Result.Interaction.GetOriginalResponseAsync(), TimeSpan.FromMinutes(1));

        if (!buttonResponse.TimedOut)
        {
            await modalReason.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Announced"));
            await buttonResponse.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .WithContent($"{ctx.TargetMember.Mention}" + $" was warned for \"{modalReason.Result.Values[reasonId]}\""
                        .RemoveDiscordMentions(ctx.Guild))
                    .AsEphemeral(false));
        }
    }

    [SlashCommand("show", "Show moderation entry with provided Id.")]
    public async Task ShowCommandAsync(
        InteractionContext ctx,
        [Option("Id", "Id of the entry to show")] long entryid)
    {
        Warning? warning = await _warningService.GetWarningAsync((int) entryid);

        if (warning is null)
        {
            await ctx.CreateResponseAsync("Entry with this Id does not exist.");
        }
        else
        {
            await ctx.CreateResponseAsync($"**Warning {warning.Id}** for user <@{warning.UserId}>:\n" +
                                          $"{warning.Reason.RemoveDiscordMentions(ctx.Guild)}");
        }
    }

    [SlashCommand("list", "List all moderation entries (For given user)")]
    public async Task ListCommandAsync(
        InteractionContext ctx,
        [Option("user", "List entries only for this user")] DiscordUser? user = null)
    {
        List<Warning> allWarnings = user is null
            ? await _warningService.GetAllWarningsAsync()
            : await _warningService.GetWarningsAsync(user.Id);
        var interactivity = ctx.Client.GetInteractivity();

        List<(string, string)> embedFields = allWarnings.ConvertAll(warning =>
        {
            string target = ctx.Guild.GetMemberAsync(warning.UserId).Result?.DisplayName ?? warning.UserId.ToString();
            string issuer = ctx.Guild.GetMemberAsync(warning.IssuerId).Result?.DisplayName ??
                            warning.IssuerId.ToString();
            return ($"#{warning.Id}\t{target}\t{warning.IssuedAt}\t{issuer}", warning.Reason);
        });

        if (!embedFields.Any())
        {
            await ctx.CreateResponseAsync("No moderation entries");
            return;
        }

        IEnumerable<Page> pages = interactivity.GeneratePages(embedFields, pageRows: 12);
        await interactivity.SendPaginatedResponseAsync(ctx.Interaction, false, ctx.User, pages);
    }

    [SlashCommand("delete", "Delete entry with provided Id")]
    public async Task DeleteCommandAsync(
        InteractionContext ctx,
        [Option("id", "Id of entry to be deleted")] long entryId)
    {
        bool success = await _warningService.DeleteWarningAsync((int)entryId);
        if (success)
        {
            await ctx.CreateResponseAsync($"Deleted entry {entryId}");
        }
        else
        {
            await ctx.CreateResponseAsync($"Failed to delete entry {entryId}, is it a valid number?", true);
        }
    }
}
