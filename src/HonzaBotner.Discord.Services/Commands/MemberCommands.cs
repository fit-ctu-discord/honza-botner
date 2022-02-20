using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using HonzaBotner.Database;
using HonzaBotner.Discord.Services.Attributes;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Commands;

[Group("member")]
[Description("Commands to interact with members.")]
[ModuleLifespan(ModuleLifespan.Transient)]
[RequireGuild]
public class MemberCommands : BaseCommandModule
{
    [Group("info")]
    [Aliases("about", "whois")]
    [Description("Provides info about a member.")]
    [RequireMod]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class MemberCommandsInfo : BaseCommandModule
    {
        private readonly HonzaBotnerDbContext _dbContext;
        private readonly IHashService _hashService;
        private readonly ILogger<MemberCommandsInfo> _logger;

        public MemberCommandsInfo(
            HonzaBotnerDbContext dbContext,
            IHashService hashService,
            ILogger<MemberCommandsInfo> logger
        )
        {
            _dbContext = dbContext;
            _hashService = hashService;
            _logger = logger;
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
            await AnnounceMemberInfoAsync(ctx, databaseRecord);
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
            await AnnounceMemberInfoAsync(ctx, databaseRecord);
        }

        private async Task AnnounceMemberInfoAsync(CommandContext ctx, Verification? databaseRecord)
        {
            if (databaseRecord == null)
            {
                await ctx.Channel.SendMessageAsync("No member record.");
                return;
            }

            try
            {
                DiscordMember? member = await ctx.Guild.GetMemberAsync(databaseRecord.UserId);
                await member.SendMessageAsync(
                    $"Member {ctx.Member.DisplayName} requested information about your account on the FIT CTU Discord server."
                );
            }
            catch (UnauthorizedException e)
            {
                _logger.LogWarning(
                    e,
                    "Couldn't send message to user {UserId} because" +
                    " the member is no longer in the guild" +
                    " or the member has Allow DM from server members off",
                    databaseRecord.UserId
                );
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Couldn't get member {MemberId}", databaseRecord.UserId);
            }

            await ctx.Channel.SendMessageAsync(databaseRecord.ToString());
        }
    }

    [Group("delete")]
    [Aliases("erase", "remove")]
    [Description("Erases database record of the member.")]
    [RequireMod]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class MemberCommandsDelete : BaseCommandModule
    {
        private readonly HonzaBotnerDbContext _dbContext;
        private readonly IHashService _hashService;
        private readonly ILogger<MemberCommandsDelete> _logger;
        private readonly IGuildProvider _guildProvider;

        public MemberCommandsDelete(
            HonzaBotnerDbContext dbContext,
            IHashService hashService,
            ILogger<MemberCommandsDelete> logger,
            IGuildProvider guildProvider
        )
        {
            _dbContext = dbContext;
            _hashService = hashService;
            _logger = logger;
            _guildProvider = guildProvider;
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
            await EraseMemberAsync(ctx, databaseRecord, member.DisplayName);
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
                await reactMessage.WaitForReactionAsync(ctx.Member, emoji, TimeSpan.FromSeconds(30));

            if (result.TimedOut)
            {
                await ctx.Channel.SendMessageAsync("Member hasn't been erased due to approval timeout.");
                return;
            }

            await ctx.TriggerTypingAsync();

            try
            {
                _dbContext.Verifications.Remove(databaseRecord);
                await _dbContext.SaveChangesAsync();
                DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();
                DiscordMember member = await guild.GetMemberAsync(databaseRecord.UserId);
                await member.RemoveAsync("This user was kicked due to being purged from the database.");
                await ctx.Channel.SendMessageAsync("Member has been erased.");
            }
            catch (UnauthorizedException)
            {
                await ctx.RespondAsync("User was purged but not kicked due to insufficient permissions\n" +
                                       "Please remove verified role manually to prevent unexpected behaviour.");
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
    [RequireAllowlist(AllowlistsTypes.MemberCount)]
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
            DiscordGuild guild = ctx.Guild;
            DiscordRole authenticatedRole = guild.GetRole(_commonCommandOptions.AuthenticatedRoleId);

            int authenticatedCount = 0;
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
                if (roles.Any(role => member.Roles.Contains(role)))
                {
                    count++;
                }
            }

            await ctx.Channel.SendMessageAsync(count.ToString());
        }

        [Command("roleAnd")]
        [Aliases("and")]
        [Description("Counts all members which have ALL of the provided roles.")]
        public async Task CountRoleAnd(
            CommandContext ctx,
            [Description("Roles to check.")] params DiscordRole[] roles
        )
        {
            int count = 0;
            DiscordGuild guild = ctx.Guild;

            foreach ((_, DiscordMember member) in guild.Members)
            {
                bool hasAllRoles = roles.All(role => member.Roles.Contains(role));

                if (hasAllRoles)
                {
                    count++;
                }
            }

            await ctx.Channel.SendMessageAsync(count.ToString());
        }
    }
}
