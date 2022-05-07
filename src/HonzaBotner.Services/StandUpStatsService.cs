using System;
using System.Threading.Tasks;
using HonzaBotner.Database;
using HonzaBotner.Services.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StandUpStat = HonzaBotner.Services.Contract.Dto.StandUpStat;

namespace HonzaBotner.Services;

public class StandUpStatsService : IStandUpStatsService
{
    private readonly HonzaBotnerDbContext _dbContext;
    private readonly ILogger<StandUpStatsService> _logger;
    private const int DaysToAcquireFreeze = 6;

    public StandUpStatsService(HonzaBotnerDbContext dbContext, ILogger<StandUpStatsService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<StandUpStat?> GetStreak(ulong userId)
    {
        Database.StandUpStat? standUpStat = await _dbContext.StandUpStats
            .FirstOrDefaultAsync(streak => streak.UserId == userId);

        return standUpStat == null ? null : GetDto(standUpStat);
    }

    public async Task UpdateStreak(ulong userId)
    {
        Database.StandUpStat? streak = await _dbContext.StandUpStats
            .FirstOrDefaultAsync(streak => streak.UserId == userId);

        // Create new streak for a new user
        if (streak is null)
        {
            Database.StandUpStat newStat = new()
            {
                UserId = userId,
                Freezes = 0,
                LastDayOfStreak = DateTime.Today.AddDays(-1),
                Streak = 1,
                LongestStreak = 1,
                LastDayCompleted = 0,
                LastDayTasks = 0,
                TotalCompleted = 0,
                TotalTasks = 0
            };

            await _dbContext.StandUpStats.AddAsync(newStat);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Couldn't add streak {@Streak}", streak);
            }

            return;
        }

        int days = (DateTime.Today.AddDays(-2) - streak.LastDayOfStreak).Days;

        if (days == -1) //Streak was already restored today
        {
            return;
        }

        if (days > streak.Freezes) //Streak broken
        {
            streak.Freezes = 0;
            streak.LastDayOfStreak = DateTime.Today.AddDays(-1);
            streak.Streak = 1;
        }
        else //streak restored
        {
            streak.Freezes -= days;
            streak.Streak++;
            streak.LastDayOfStreak = DateTime.Today.AddDays(-1);

            if (streak.Streak > streak.LongestStreak)
            {
                streak.LongestStreak = streak.Streak;
            }

            if (streak.Streak % DaysToAcquireFreeze == 0) // freeze acquired
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

    public async Task<bool> IsValidStreak(ulong userId)
    {
        StandUpStat? streak = await GetStreak(userId);

        if (streak is null)
            return false;
        int days = (DateTime.Today.AddDays(-2) - streak.LastDayOfStreak).Days;

        return days <= streak.Freezes;
    }

    public async Task UpdateStats(ulong userId, int completed, int total) {
        Database.StandUpStat? stat = await _dbContext.StandUpStats
            .FirstOrDefaultAsync(streak => streak.UserId == userId);

        if (stat is null)
        {
            Database.StandUpStat newStat = new()
            {
                UserId = userId,
                Freezes = 0,
                LastDayOfStreak = DateTime.Today.AddDays(-1),
                Streak = 1,
                LongestStreak = 1,
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

private static StandUpStat GetDto(Database.StandUpStat standUpStat) =>
        new StandUpStat()
        {
            Id = standUpStat.Id,
            UserId = standUpStat.UserId,
            Streak = standUpStat.Streak,
            LongestStreak = standUpStat.LongestStreak,
            Freezes = standUpStat.Freezes,
            LastDayOfStreak = standUpStat.LastDayOfStreak
        };
}
