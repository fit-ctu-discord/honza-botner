using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using HonzaBotner.Discord.Extensions;
using HonzaBotner.Discord.Services.Attributes;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("warning")]
    [Aliases("warn", "takheledebilku")]
    [Description("Commands to warn and list warnings.")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [RequireMod]
    public class WarningCommands : BaseCommandModule
    {
        private readonly IWarningService _warningService;
        private readonly ILogger<WarningCommands> _logger;

        public WarningCommands(IWarningService warningService, ILogger<WarningCommands> logger)
        {
            _warningService = warningService;
            _logger = logger;
        }

        [GroupCommand]
        [Command("add")]
        [Aliases("new", "issue")]
        [Description("Add a new warning to the user.")]
        public async Task AddWarning(
            CommandContext ctx,
            [Description("Member to warn.")] DiscordMember member,
            [Description("Reason for issuing a warning.")] [RemainingText]
            string reason
        )
        {
            await ctx.TriggerTypingAsync();
            await WarnUserAsync(ctx, member, reason, false, ctx.Member);
        }

        [Command("silent")]
        [Description("Add a new warning privately.")]
        public async Task AddSilentWarning(
            CommandContext ctx,
            [Description("Member to warn.")] DiscordMember member,
            [Description("Reason for issuing a warning.")] [RemainingText]
            string reason
        )
        {
            await ctx.TriggerTypingAsync();
            await WarnUserAsync(ctx, member, reason, true, ctx.Member);
        }

        [Command("list")]
        [Aliases("print")]
        [Description("Lists all warnings.")]
        public async Task ListWarnings(
            CommandContext ctx
        )
        {
            await ctx.TriggerTypingAsync();

            List<Warning> allWarnings = await _warningService.GetAllWarningsAsync();
            await PrintWarningListAsync(ctx, allWarnings);
        }

        [Command("list")]
        public async Task ListWarnings(
            CommandContext ctx,
            [Description("Member to list.")] DiscordMember member
        )
        {
            await ctx.TriggerTypingAsync();

            List<Warning> allWarnings = await _warningService.GetWarningsAsync(member.Id);
            await PrintWarningListAsync(ctx, allWarnings, member);
        }

        [Command("show")]
        public async Task ShowWarning(
            CommandContext ctx,
            [Description("Warning id to show.")] int id
        )
        {
            await ctx.TriggerTypingAsync();

            Warning? warning = await _warningService.GetWarningAsync(id);

            if (warning == null)
            {
                try
                {
                    await ctx.Channel.SendMessageAsync("Varování s tímto ID neexistuje.");
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Couldn't send a message");
                }

                return;
            }

            try
            {
                DiscordMember warningMember = await ctx.Guild.GetMemberAsync(warning.UserId);
                await ctx.Channel.SendMessageAsync(
                    $"**Varování {warning.Id}** pro uživatele **{warningMember.RatherNicknameThanUsername()}**:\n" +
                    $"{warning.Reason.RemoveDiscordMentions(ctx.Guild, _logger)}");
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":+1:"));
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Couldn't send warning message");
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":-1:"));
            }
        }

        [Command("delete")]
        [Aliases("erase", "remove")]
        public async Task DeleteWarning(
            CommandContext ctx,
            [Description("Warning id to delete.")] int id
        )
        {
            await ctx.TriggerTypingAsync();

            bool success = await _warningService.DeleteWarningAsync(id);

            if (success)
            {
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":+1:"));
            }
            else
            {
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":-1:"));
            }
        }

        private async Task WarnUserAsync(
            CommandContext ctx,
            DiscordMember member,
            string reason,
            bool silent,
            DiscordMember issuer
        )
        {
            int? warningId = await _warningService.AddWarningAsync(member.Id, reason, issuer.Id);

            if (warningId.HasValue)
            {
                int numberOfWarnings = await _warningService.GetNumberOfWarnings(member.Id);

                try
                {
                    await ctx.Message.DeleteAsync();

                    if (silent)
                    {
                        await ctx.Channel.SendMessageAsync(
                            $"**Uživatel <@!{member.Id}> byl varován!**");
                    }
                    else
                    {
                        await ctx.Channel.SendMessageAsync(
                            $"**Varování (<@!{member.Id}>)**: {reason.RemoveDiscordMentions(ctx.Guild, _logger)}.");
                    }

                    string messageForUser =
                        $"Na *studentském FIT Discordu* v kanále _#{ctx.Channel.Name}_ vám bylo uděleno **varování**. (warning id: {warningId})" +
                        $"\n\n" +
                        $"Důvod: {reason}" +
                        "\n\n" +
                        $"Toto je vaše **{numberOfWarnings}. varování**, #beGood.";

                    await member.SendMessageAsync(messageForUser.RemoveDiscordMentions(ctx.Guild, _logger));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to announce warning");
                }
            }
            else
            {
                _logger.LogWarning("Couldn't add a warning for {Member} with {Reason}",
                    member.RatherNicknameThanUsername(), reason);
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":-1:"));
            }
        }

        private async Task PrintWarningListAsync(CommandContext ctx, List<Warning> warnings,
            DiscordMember? forMember = null)
        {
            InteractivityExtension? interactivity = ctx.Client.GetInteractivity();
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = forMember == null
                    ? "Seznam varování"
                    : $"Seznam varování uživatele {forMember.RatherNicknameThanUsername()}",
            };

            List<(string, string)> embedFields = new();

            foreach (Warning warning in warnings)
            {
                try
                {
                    DiscordMember warningMember = await ctx.Guild.GetMemberAsync(warning.UserId);
                    DiscordMember issuerMember = await ctx.Guild.GetMemberAsync(warning.IssuerId);

                    embedFields.Add(
                        ($"#{warning.Id}\t{warningMember.RatherNicknameThanUsername()}\t{warning.IssuedAt}\t{issuerMember.RatherNicknameThanUsername()}",
                            warning.Reason)
                    );
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Couldn't fetch user or issuer");
                }
            }

            IEnumerable<Page> pages = interactivity.GeneratePages(embedFields, embedBuilder, 12);
            await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, pages);
        }
    }
}
