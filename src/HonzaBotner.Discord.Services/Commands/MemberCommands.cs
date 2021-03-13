using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using HonzaBotner.Database;
using HonzaBotner.Discord.Services.Attributes;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("member")]
    [Description("Commands to interact with members.")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [RequireMod]
    public class MemberCommands : BaseCommandModule
    {
        private readonly HonzaBotnerDbContext _dbContext;
        private readonly IHashService _hashService;
        private readonly ILogger<MemberCommands> _logger;

        public MemberCommands(HonzaBotnerDbContext dbContext, IHashService hashService, ILogger<MemberCommands> logger)
        {
            _dbContext = dbContext;
            _hashService = hashService;
            _logger = logger;
        }

        [Command("info")]
        [Aliases("about", "whois")]
        [Description("Provides info about a member.")]
        [Priority(2)]
        public async Task MemberInfo(CommandContext ctx,
            [Description("Member to show info about.")]
            DiscordMember member)
        {
            Verification? databaseRecord = await _dbContext.Verifications
                .FirstOrDefaultAsync(v => v.UserId == member.Id);
            await MemberInfoAsync(ctx, databaseRecord);
        }

        [Command("info")]
        [Priority(1)]
        public async Task MemberInfo(CommandContext ctx, [Description("CVUT username.")] string cvutUsername)
        {
            string authId = _hashService.Hash(cvutUsername);
            Verification? databaseRecord = await _dbContext.Verifications.FirstOrDefaultAsync(v => v.AuthId == authId);
            await MemberInfoAsync(ctx, databaseRecord);
        }

        [Command("delete")]
        [Aliases("erase", "remove")]
        [Description("Erases database record of the member.")]
        [Priority(2)]
        public async Task MemberErase(CommandContext ctx,
            [Description("Discord member to erase.")]
            DiscordMember member)
        {
            Verification? databaseRecord =
                await _dbContext.Verifications.FirstOrDefaultAsync(v => v.UserId == member.Id);
            await EraseMemberAsync(ctx, databaseRecord, member.Nickname ?? member.Username);
        }

        [Command("erase")]
        [Priority(1)]
        public async Task MemberErase(CommandContext ctx,
            [Description("CVUT username of member to erase.")]
            string cvutUsername)
        {
            string authId = _hashService.Hash(cvutUsername);
            Verification? databaseRecord = await _dbContext.Verifications.FirstOrDefaultAsync(v => v.AuthId == authId);
            await EraseMemberAsync(ctx, databaseRecord, cvutUsername);
        }

        [Group(name: "count")]
        [Description("Counts members by provided roles.")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class MemberRoleCount : BaseCommandModule
        {
            private readonly CommonCommandOptions _commonCommandOptions;

            public MemberRoleCount(IOptions<CommonCommandOptions> commonCommandOptions)
            {
                _commonCommandOptions = commonCommandOptions.Value;
            }

            [Command("all")]
            [Description("Counts all members and all authenticated members.")]
            public async Task CountAll(CommandContext ctx)
            {
                int authenticatedCount = 0;
                DiscordRole authenticatedRole = ctx.Guild.GetRole(_commonCommandOptions.AuthenticatedRoleId);

                foreach ((_, DiscordMember member) in ctx.Guild.Members)
                {
                    if (member.Roles.Contains(authenticatedRole))
                    {
                        authenticatedCount++;
                    }
                }

                await ctx.Channel.SendMessageAsync(
                    $"Authenticated: {authenticatedCount}, All: {ctx.Guild.Members.Count.ToString()}");
            }

            [GroupCommand]
            [Command("roleOr")]
            [Aliases("or")]
            [Description("Counts all members which have AT LEAST ONE of the provided roles.")]
            public async Task CountRoleOr(CommandContext ctx,
                [Description("Roles to check.")] params DiscordRole[] roles)
            {
                int count = 0;

                foreach ((_, DiscordMember member) in ctx.Guild.Members)
                {
                    foreach (DiscordRole role in roles)
                    {
                        if (member.Roles.Contains(role))
                        {
                            count++;
                            break;
                        }
                    }
                }

                await ctx.Channel.SendMessageAsync(count.ToString());
            }

            [Command("roleAnd")]
            [Aliases("and")]
            [Description("Counts all members which have ALL of the provided roles.")]
            public async Task CountRoleAnd(CommandContext ctx,
                [Description("Roles to check.")] params DiscordRole[] roles)
            {
                int count = 0;

                foreach ((_, DiscordMember member) in ctx.Guild.Members)
                {
                    bool hasAllRoles = true;
                    foreach (DiscordRole role in roles)
                    {
                        if (!member.Roles.Contains(role))
                        {
                            hasAllRoles = false;
                            break;
                        }
                    }

                    if (hasAllRoles)
                    {
                        count++;
                    }
                }

                await ctx.Channel.SendMessageAsync(count.ToString());
            }
        }

        private async Task EraseMemberAsync(CommandContext ctx, Verification? databaseRecord, string userName)
        {
            await ctx.TriggerTypingAsync();
            if (databaseRecord == null)
            {
                await ctx.Channel.SendMessageAsync("No member record to erase.");
                return;
            }

            DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":ok_hand:");
            DiscordMessage reactMessage =
                await ctx.Channel.SendMessageAsync($"To approve erase of `{userName}`, react with {emoji}");
            await reactMessage.CreateReactionAsync(emoji);
            InteractivityResult<MessageReactionAddEventArgs> result =
                await reactMessage.WaitForReactionAsync(ctx.Member, emoji);

            await ctx.TriggerTypingAsync();
            if (result.TimedOut)
            {
                await ctx.Channel.SendMessageAsync("Member hasn't been erased due to approval timeout.");
                return;
            }

            try
            {
                _dbContext.Verifications.Remove(databaseRecord);
                await _dbContext.SaveChangesAsync();
                await ctx.Channel.SendMessageAsync("Member has been erased.");
            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync("Member erase failed.");
                _logger.LogWarning(e, "Member erase failed");
            }
        }

        private async Task MemberInfoAsync(CommandContext ctx, Verification? databaseRecord)
        {
            await ctx.Channel.SendMessageAsync(databaseRecord?.ToString() ?? "No member record.");
        }
    }
}
