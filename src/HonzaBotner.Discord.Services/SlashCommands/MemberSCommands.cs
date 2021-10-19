using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using HonzaBotner.Database;
using HonzaBotner.Services.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.SlashCommands
{
    [SlashCommandGroup("member", "Commands to access or modify member information")]
    public class MemberSCommands : ApplicationCommandModule
    {
        private readonly HonzaBotnerDbContext _dbContext;
        private readonly IHashService _hashService;
        private readonly ILogger<MemberSCommands> _logger;

        public MemberSCommands(
            HonzaBotnerDbContext dbContext,
            IHashService hashService,
            ILogger<MemberSCommands> logger)
        {
            _dbContext = dbContext;
            _hashService = hashService;
            _logger = logger;
        }

        [SlashCommand("info", "Provides info about a member")]
        [SlashRequireGuild]
        public async Task MemberInfoAsync(
            InteractionContext ctx,
            [Option("Discord member", "Member whose information you want to access")]
            DiscordMember? member = null,
            [Option("CVUT username", "Username of the person whose info you want to access")]
            string? userName = null)
        {
            Verification? databaseRecord;

            if (member is not null)
            {
                databaseRecord = await _dbContext.Verifications
                    .FirstOrDefaultAsync(v => v.UserId == member.Id);
            }
            else if (userName is not null)
            {
                string authId = _hashService.Hash(userName);
                databaseRecord =
                    await _dbContext.Verifications.FirstOrDefaultAsync(v => v.AuthId == authId);
            }
            else
            {
                databaseRecord = await _dbContext.Verifications
                    .FirstOrDefaultAsync(v => v.UserId == ctx.User.Id);
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent(databaseRecord?.ToString() ?? "No member record."));

        }

    }
}
