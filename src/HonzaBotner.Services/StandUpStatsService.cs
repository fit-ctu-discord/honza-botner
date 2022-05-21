using System;
using System.Threading.Tasks;
using HonzaBotner.Database;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StandUpStat = HonzaBotner.Services.Contract.Dto.StandUpStat;

namespace HonzaBotner.Services;

public class StandUpStatsService : IStandUpStatsService
{
    private readonly HonzaBotnerDbContext _dbContext;
    private readonly ILogger<StandUpStatsService> _logger;
    private readonly StandUpOptions _standUpOptions;

    private const int ComparedDay = -1;

    public StandUpStatsService(
        HonzaBotnerDbContext dbContext,
        ILogger<StandUpStatsService> logger,
        IOptions<StandUpOptions> standUpOptions
    )
    {
        _dbContext = dbContext;
        _logger = logger;
        _standUpOptions = standUpOptions.Value;
    }

    public async Task<StandUpStat?> GetStreak(ulong userId)
    {
        Database.StandUpStat? standUpStat = await _dbContext.StandUpStats
            .FirstOrDefaultAsync(streak => streak.UserId == userId);

        return standUpStat == null ? null : GetDto(standUpStat);
    }

    public async Task UpdateStreak(ulong userId, Database.StandUpStat streak)
    {
        int days = (DateTime.Today.AddDays(-2) - streak.LastDayOfStreak).Days;

        if (days > streak.Freezes) //Streak broken
        {
            streak.Freezes = 0;
            streak.LastDayOfStreak = DateTime.Today.AddDays(-1).ToUniversalTime();
            streak.Streak = 1;
        }
        else //streak restored
        {
            streak.Freezes--;
            streak.Streak++;
            streak.LastDayOfStreak = DateTime.Today.AddDays(-1).ToUniversalTime();

            if (streak.Streak > streak.LongestStreak)
            {
                streak.LongestStreak = streak.Streak;
            }

            // Freeze acquired.
            if (streak.Streak % _standUpOptions.DaysToAcquireFreeze == 0)
            {
                streak.Freezes++;
            }
        }

        try
        {
            _dbContext.StandUpStats.Update(streak);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Couldn't update streak {@Streak}", streak);
        }
    }

    public async Task UpdateStats(ulong userId, int completed, int total)
    {
        Database.StandUpStat? stat = await _dbContext.StandUpStats
            .FirstOrDefaultAsync(streak => streak.UserId == userId);

        if (stat is null)
        {
            Database.StandUpStat newStat = new()
            {
                UserId = userId,
                Freezes = 0,
                LastDayOfStreak =
                    completed != 0
                        ? DateTime.Today.AddDays(-1).ToUniversalTime()
                        : DateTime.Today.AddDays(-100).ToUniversalTime(),
                Streak = completed != 0 ? 1 : 0,
                LongestStreak = completed != 0 ? 1 : 0,
                LastDayCompleted = completed,
                LastDayTasks = total,
                TotalCompleted = completed,
                TotalTasks = total
            };

            await _dbContext.StandUpStats.AddAsync(newStat);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Couldn't add streak {@Stat}", stat);
            }

            return;
        }

        if (completed != 0)
        {
            await UpdateStreak(userId, stat);
        }

        stat.LastDayCompleted = completed;
        stat.LastDayTasks = total;
        stat.TotalCompleted += completed;
        stat.TotalTasks += total;

        try
        {
            _dbContext.StandUpStats.Update(stat);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Couldn't update streak {@Stat}", stat);
        }
    }

    static StandUpStat GetDto(Database.StandUpStat standUpStat) =>
        new(
            standUpStat.Id,
            standUpStat.UserId,
            standUpStat.Streak,
            standUpStat.LongestStreak,
            standUpStat.Freezes,
            standUpStat.LastDayOfStreak,
            standUpStat.TotalCompleted,
            standUpStat.TotalTasks
        );
}
