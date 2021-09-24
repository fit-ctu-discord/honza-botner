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
    [RequireGuild]
    public class MemberCommands : BaseCommandModule
    {
        [Group("info")]
        [Aliases("about", "whois")]
        [Description("Provides info about a member.")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class MemberCommandsInfo : BaseCommandModule
        {
            private readonly HonzaBotnerDbContext _dbContext;
            private readonly IHashService _hashService;

            public MemberCommandsInfo(
                HonzaBotnerDbContext dbContext,
                IHashService hashService
            )
            {
                _dbContext = dbContext;
                _hashService = hashService;
            }

            [GroupCommand]
            [Command("discord")]
            [Description("Provides info about a member by Discord account.")]
            public async Task DiscordMemberInfo(
                CommandContext ctx,
                [Description("Member to show info about.")]
                DiscordMember member
            )
            {
                Verification? databaseRecord = await _dbContext.Verifications
                    .FirstOrDefaultAsync(v => v.UserId == member.Id);
                await MemberInfoAsync(ctx, databaseRecord);
            }

            [Command("ctu")]
            [Aliases("cvut")]
            [Description("Provides info about a member by CTU username.")]
            public async Task CtuMemberInfo(
                CommandContext ctx,
                [Description("CTU username.")] string cvutUsername
            )
            {
                string authId = _hashService.Hash(cvutUsername);
                Verification? databaseRecord =
                    await _dbContext.Verifications.FirstOrDefaultAsync(v => v.AuthId == authId);
                await MemberInfoAsync(ctx, databaseRecord);
            }

            private async Task MemberInfoAsync(CommandContext ctx, Verification? databaseRecord)
            {
                await ctx.Channel.SendMessageAsync(databaseRecord?.ToString() ?? "No member record.");
            }
        }


        [Group("delete")]
        [Aliases("erase", "remove")]
        [Description("Erases database record of the member.")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class MemberCommandsDelete : BaseCommandModule
        {
            private readonly HonzaBotnerDbContext _dbContext;
            private readonly IHashService _hashService;
            private readonly ILogger<MemberCommandsDelete> _logger;

            public MemberCommandsDelete(
                HonzaBotnerDbContext dbContext,
                IHashService hashService,
                ILogger<MemberCommandsDelete> logger
            )
            {
                _dbContext = dbContext;
                _hashService = hashService;
                _logger = logger;
            }

            [GroupCommand]
            [Command("discord")]
            [Description("Erases database record of the member by Discord account.")]
            public async Task DiscordMemberErase(
                CommandContext ctx,
                [Description("Discord member to erase.")]
                DiscordMember member
            )
            {
                Verification? databaseRecord =
                    await _dbContext.Verifications.FirstOrDefaultAsync(v => v.UserId == member.Id);
                await EraseMemberAsync(ctx, databaseRecord, member.Nickname ?? member.Username);
            }

            [Command("ctu")]
            [Aliases("cvut")]
            [Description("Erases database record of the member by CTU username.")]
            public async Task CtuMemberErase(
                CommandContext ctx,
                [Description("CVUT username of member to erase.")]
                string cvutUsername
            )
            {
                string authId = _hashService.Hash(cvutUsername);
                Verification? databaseRecord =
                    await _dbContext.Verifications.FirstOrDefaultAsync(v => v.AuthId == authId);
                await EraseMemberAsync(ctx, databaseRecord, cvutUsername);
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
        }

        [Group("count")]
        [Description("Counts members by provided roles.")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class MemberRoleCount : BaseCommandModule
        {
            private readonly CommonCommandOptions _commonCommandOptions;

            public MemberRoleCount(
                IOptions<CommonCommandOptions> commonCommandOptions
            )
            {
                _commonCommandOptions = commonCommandOptions.Value;
            }

            [Command("all")]
            [Description("Counts all members and all authenticated members.")]
            public async Task CountAll(CommandContext ctx)
            {
                int authenticatedCount = 0;
                DiscordGuild guild = ctx.Guild;
                DiscordRole authenticatedRole = guild.GetRole(_commonCommandOptions.AuthenticatedRoleId);

                foreach ((_, DiscordMember member) in guild.Members)
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
            public async Task CountRoleOr(
                CommandContext ctx,
                [Description("Roles to check.")] params DiscordRole[] roles
            )
            {
                int count = 0;
                DiscordGuild guild = ctx.Guild;

                foreach ((_, DiscordMember member) in guild.Members)
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
                DiscordGuild guild = ctx.Guild;

                foreach ((_, DiscordMember member) in guild.Members)
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
    }
}
