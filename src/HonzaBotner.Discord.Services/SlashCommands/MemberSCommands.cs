using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using HonzaBotner.Database;
using HonzaBotner.Services.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.SlashCommands
{
    [SlashCommandGroup("member", "Commands to access or modify member information")]
    public class MemberSCommands : ApplicationCommandModule
    {
        [SlashCommandGroup("info", "Provides info about a member")]
        public class MemberSCommandsInfo : ApplicationCommandModule
        {
            private readonly HonzaBotnerDbContext _dbContext;
            private readonly IHashService _hashService;

            public MemberSCommandsInfo(
                HonzaBotnerDbContext dbContext,
                IHashService hashService)
            {
                _dbContext = dbContext;
                _hashService = hashService;
            }

            [SlashCommand("discord", "Gets info about discord member from his discord username")]
            public async Task MemberInfoAsync(
                InteractionContext ctx,
                [Option("nickname", "Member whose information you want to access")]
                DiscordUser user)

            {
                Verification? databaseRecord = await _dbContext.Verifications
                        .FirstOrDefaultAsync(v => v.UserId == user.Id);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent(databaseRecord?.ToString() ?? "No member record."));

            }

            [SlashCommand("usermap", "Gets info about discord member from his usermap username")]
            public async Task UsermapInfoAsync(
                InteractionContext ctx,
                [Option("nickname", "Username of the person whose info you want to access")]
                string userName)
            {
                string authId = _hashService.Hash(userName);
                Verification databaseRecord =
                    await _dbContext.Verifications.FirstOrDefaultAsync(v => v.AuthId == authId);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent(databaseRecord?.ToString() ?? "No member record."));

            }
        }

        [SlashCommandGroup("erase", "Removes person from bot's database")]
        public class MemberSCommandsDelete : ApplicationCommandModule
        {
            private readonly HonzaBotnerDbContext _dbContext;
            private readonly IHashService _hashService;
            private readonly ILogger<MemberSCommandsDelete> _logger;
            private readonly IGuildProvider _guildProvider;

            public MemberSCommandsDelete(
                HonzaBotnerDbContext dbContext,
                IHashService hashService,
                ILogger<MemberSCommandsDelete> logger,
                IGuildProvider guildProvider
            )
            {
                _dbContext = dbContext;
                _hashService = hashService;
                _logger = logger;
                _guildProvider = guildProvider;
            }

            [SlashCommand("discord", "Erases member by his Discord name")]
            public async Task MemberEraseAsync(
                InteractionContext ctx,
                [Option("DiscordUser","Discord User you want to erase")]
                DiscordUser user
            )
            {
                Verification? databaseRecord =
                    await _dbContext.Verifications.FirstOrDefaultAsync(v => v.UserId == user.Id);
                await EraseAsync(ctx, databaseRecord, user.Username);
            }

            [SlashCommand("ctu", "Erases member by his usermap nickname")]
            public async Task CtuEraseAsync(
                InteractionContext ctx,
                [Option("UsermapName","User's name in system")]
                string cvutUsername
            )
            {
                string authId = _hashService.Hash(cvutUsername);
                Verification? databaseRecord =
                    await _dbContext.Verifications.FirstOrDefaultAsync(v => v.AuthId == authId);
                await EraseAsync(ctx, databaseRecord, cvutUsername);
            }

            private async Task EraseAsync(InteractionContext ctx, Verification? databaseRecord, string target)
            {
                if (databaseRecord is null)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent("No member record to erase."));
                    return;
                }

                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":ok_hand:");
                DiscordMessage reactMessage =
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                        .WithContent($"To approve erasure of `{target}`, react with {emoji}"));
                await reactMessage.CreateReactionAsync(emoji);
                InteractivityResult<MessageReactionAddEventArgs> result =
                    await reactMessage.WaitForReactionAsync(ctx.Member, emoji, TimeSpan.FromSeconds(30));

                if (result.TimedOut)
                {
                    await ctx.DeleteFollowupAsync(reactMessage.Id);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent("Member hasn't been erased due to approval timeout."));
                    return;
                }

                _dbContext.Verifications.Remove(databaseRecord);
                await _dbContext.SaveChangesAsync();
                DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();
                DiscordMember member = await guild.GetMemberAsync(databaseRecord.UserId);
                await member.ReplaceRolesAsync(Enumerable.Empty<DiscordRole>());

                await ctx.DeleteFollowupAsync(reactMessage.Id);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"Erasure of {target} was approved by {ctx.User.Mention}.\nMember erased."));
            }
        }

        [SlashCommand("count", "Count all members")]
        public async Task countAsync(InteractionContext ctx,
            [Option("role", "Count only members with a given role")]
            DiscordRole? role = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.Pong);
        }

    }
}
