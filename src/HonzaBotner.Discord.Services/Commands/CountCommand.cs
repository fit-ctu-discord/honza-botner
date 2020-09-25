using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Database;
using HonzaBotner.Discord.Command;
using Microsoft.EntityFrameworkCore;

namespace HonzaBotner.Discord.Services.Commands
{
    public class CountCommand : IChatCommand
    {
        public const string ChatCommand = "counterBump";

        private readonly HonzaBotnerDbContext _dbContext;

        public CountCommand(HonzaBotnerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ChatCommendExecutedResult> ExecuteAsync(DiscordClient client, DiscordMessage message, CancellationToken cancellationToken = default)
        {
            ulong userId = message.Author.Id;
            Counter? counter = await _dbContext.Counters.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

            if (counter == null)
            {
                await _dbContext.Counters.AddAsync(new Counter { UserId = userId, Count = 0 }, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await message.RespondAsync("Created new counter! Try again");
                return ChatCommendExecutedResult.Ok;
            }

            counter.Count++;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await message.RespondAsync($"Current value of your counter is: {counter.Count}");
            return ChatCommendExecutedResult.Ok;
        }
    }
}
