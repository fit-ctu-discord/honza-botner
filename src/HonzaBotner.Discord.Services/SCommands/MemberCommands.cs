using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using HonzaBotner.Database;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.SCommands;

[SlashCommandGroup("member", "member commands")]
[GuildOnly]
[SlashCommandPermissions(Permissions.ModerateMembers)]
public class MemberCommands : ApplicationCommandModule
{

    [SlashCommandGroup("info", "Provides info about a member.")]
    public class MemberCommandsInfo : ApplicationCommandModule
    {

        private readonly HonzaBotnerDbContext _dbContext;
        private readonly IHashService _hashService;
        private readonly ILogger<MemberCommandsInfo> _logger;

        public MemberCommandsInfo(
            HonzaBotnerDbContext dbContext,
            IHashService hashService,
            ILogger<MemberCommandsInfo> logger)
        {
            _dbContext = dbContext;
            _hashService = hashService;
            _logger = logger;
        }

        [SlashCommand("discord", "Provides info about a member by Discord account.")]
        public async Task DiscordMemberInfoCommandAsync(
            InteractionContext ctx,
            [Option("member","Who is the target?")] DiscordUser user)
        {
            Verification? databaseRecord = await _dbContext.Verifications
                .FirstOrDefaultAsync(v => v.UserId == user.Id);
            await AnnounceMemberInfoAsync(ctx, databaseRecord);
        }

        [SlashCommand("ctu", "Provides info about a member by CTU username.")]
        public async Task CtuMemberInfoCommandAsync(
            InteractionContext ctx,
            [Option("username", "Who is the target?")] string cvutUsername)
        {
            string authId = _hashService.Hash(cvutUsername);
            Verification? databaseRecord = await _dbContext.Verifications
                .FirstOrDefaultAsync(v => v.AuthId == authId);
            await AnnounceMemberInfoAsync(ctx, databaseRecord);
        }

        private async Task AnnounceMemberInfoAsync(InteractionContext ctx, Verification? databaseRecord)
        {
            if (databaseRecord == null)
            {
                await ctx.CreateResponseAsync("No member record for provided name");
                return;
            }

            try
            {
                DiscordMember member = await ctx.Guild.GetMemberAsync(databaseRecord.UserId);
                await member.SendMessageAsync($"Member {ctx.Member.DisplayName} requested information about your account on the FIT CTU Discord server.");
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Couldn't get member {MemberId}", databaseRecord.UserId);
            }

            await ctx.CreateResponseAsync(databaseRecord.ToString());
        }
    }

    [SlashCommandGroup("delete", "Erases database record of the member.")]
    public class MemberCommandsDelete : ApplicationCommandModule
    {
        private readonly HonzaBotnerDbContext _dbContext;
        private readonly IHashService _hashService;
        private readonly ILogger<MemberCommandsDelete> _logger;
        private readonly IGuildProvider _guildProvider;

        public MemberCommandsDelete(
            HonzaBotnerDbContext dbContext,
            IHashService hashService,
            ILogger<MemberCommandsDelete> logger,
            IGuildProvider guildProvider)
        {
            _dbContext = dbContext;
            _hashService = hashService;
            _logger = logger;
            _guildProvider = guildProvider;
        }

        [SlashCommand("discord", "Erases database record of the member by Discord account.")]
        public async Task DiscordMemberEraseCommandAsync(
            InteractionContext ctx,
            [Option("member", "Who is the target?")] DiscordUser user)
        {
            Verification? databaseRecord = await _dbContext.Verifications
                .FirstOrDefaultAsync(v => v.UserId == user.Id);
            await EraseMemberAsync(ctx, databaseRecord);
        }

        [SlashCommand("ctu", "Erases database record of the member by CTU username.")]
        public async Task CtuMemberEraseCommandAsync(
            InteractionContext ctx,
            [Option("username", "Who is the target?")] string cvutUsername)
        {
            string authId = _hashService.Hash(cvutUsername);
            Verification? databaseRecord = await _dbContext.Verifications
                .FirstOrDefaultAsync(v => v.AuthId == authId);
            await EraseMemberAsync(ctx, databaseRecord);
        }

        private async Task EraseMemberAsync(InteractionContext ctx, Verification? databaseRecord)
        {
            await ctx.DeferAsync();
            if (databaseRecord == null)
            {
                await ctx.CreateResponseAsync("No member record to erase.");
                return;
            }

            try
            {
                _dbContext.Verifications.Remove(databaseRecord);
                await _dbContext.SaveChangesAsync();
                DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();
                DiscordMember member = await guild.GetMemberAsync(databaseRecord.UserId);
                await member.RemoveAsync("This user was kicked due to being purged from the database.");
                await ctx.CreateResponseAsync("Member has been erased.");
                return;
            }
            catch (UnauthorizedException e)
            {
                _logger.LogWarning(e, "Erasing of member failed due to lack of permissions");
                await ctx.CreateResponseAsync("User was purged but not kicked due to insufficient permissions\n" +
                                       "Please remove verified role manually to prevent unexpected behaviour.");
            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync("Member erase failed.");
                _logger.LogWarning(e, "Member erase failed");
            }

            await ctx.CreateResponseAsync("Failed to erase member. More information in the log.");
        }
    }

    [SlashCommandGroup("count", "Counts members by provided roles.")]
    public class MemberCommandsCount : ApplicationCommandModule
    {
        private readonly CommonCommandOptions _commonCommandOptions;

        public MemberCommandsCount(IOptions<CommonCommandOptions> commonCommandOptions)
        {
            _commonCommandOptions = commonCommandOptions.Value;
        }

        [SlashCommand("all", "Counts all members and all authenticated members.")]
        public async Task CountAllCommandAsync(InteractionContext ctx)
        {
            DiscordGuild guild = ctx.Guild;
            DiscordRole authenticatedRole = guild.GetRole(_commonCommandOptions.AuthenticatedRoleId);

            int authenticatedCount = guild.Members.Count(member => member.Value.Roles.Contains(authenticatedRole));
            await ctx.CreateResponseAsync($"Authenticated: {authenticatedCount}, All: {ctx.Guild.Members.Count.ToString()}");
        }

        [SlashCommand("here", "Counts all members who can see this channel")]
        public async Task CountHereCommandAsync(
            InteractionContext ctx,
            [Option("ephemeral", "Hide response? Default false")] bool ephemeral = false)
        {
            await ctx.CreateResponseAsync($"Members in this channel: {ctx.Channel.Users.Count}", ephemeral);
        }

        [SlashCommand("role", "Counts members based on provided roles")]
        public async Task CountRoleCommandAsync(
            InteractionContext ctx,
            [Option("roles", "Roles to look for - write as a mention")] string roles,
            [Choice("and", "and")]
            [Choice("or", "or")]
            [Option("search-type", "Look for members with all those roles/some. Default: and")] string type = "and")
        {
            int count = 0;
            DiscordGuild guild = ctx.Guild;

            await ctx.DeferAsync();

            foreach ((_, DiscordMember member) in guild.Members)
            {
                switch (type)
                {
                    case "or" when ctx.ResolvedRoleMentions.Any(role => member.Roles.Contains(role)):
                    case "and" when ctx.ResolvedRoleMentions.All(role => member.Roles.Contains(role)):
                        count++;
                        break;
                }
            }

            await ctx.Channel.SendMessageAsync(count.ToString());
        }
    }
}
