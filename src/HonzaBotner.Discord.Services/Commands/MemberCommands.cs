using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Database;
using HonzaBotner.Discord.Services.Attributes;
using HonzaBotner.Services.Contract;
using Microsoft.EntityFrameworkCore;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("member")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [Description("Member commands.")]
    [Hidden]
    [RequireMod]
    public class MemberCommands : BaseCommandModule
    {
        private readonly HonzaBotnerDbContext _dbContext;
        private readonly IHashService _hashService;

        public MemberCommands(HonzaBotnerDbContext dbContext, IHashService hashService)
        {
            _dbContext = dbContext;
            _hashService = hashService;
        }

        [Command("info")]
        [Priority(1)]
        public async Task MemberInfo(CommandContext ctx, DiscordMember member)
        {
            Verification databaseRecord = await _dbContext.Verifications.FirstAsync(v => v.UserId == member.Id);
            await ctx.Channel.SendMessageAsync(databaseRecord.ToString());
        }

        [Command("info")]
        [Priority(2)]
        public async Task MemberInfo(CommandContext ctx, string cvutUsername)
        {
            string authId = _hashService.Hash(cvutUsername);
            Verification databaseRecord = await _dbContext.Verifications.FirstAsync(v => v.AuthId == authId);
            await ctx.Channel.SendMessageAsync(databaseRecord.ToString());
        }

        [Command("erase")]
        public async Task MemberErase(CommandContext ctx, DiscordMember member)
        {
            Verification databaseRecord = await _dbContext.Verifications.FirstAsync(v => v.UserId == member.Id);

            _dbContext.Verifications.Remove(databaseRecord);
            await _dbContext.SaveChangesAsync();
        }

        [Group(name: "role")]
        public class MemberRoleCount : BaseCommandModule
        {
            [Command("count")]
            public async Task CountRole(CommandContext ctx, DiscordRole role)
            {
                int count = 0;

                foreach ((_, DiscordMember member) in ctx.Guild.Members)
                {
                    if (member.Roles.Contains(role))
                    {
                        count++;
                    }
                }

                await ctx.Channel.SendMessageAsync(count.ToString());
            }
        }
    }
}
