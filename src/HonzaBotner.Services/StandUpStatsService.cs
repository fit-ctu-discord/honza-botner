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

    public async Task<StandUpStat?> GetStats(ulong userId)
    {
        Database.StandUpStat? standUpStat = await _dbContext.StandUpStats
            .FirstOrDefaultAsync(streak => streak.UserId == userId);

        return standUpStat == null ? null : GetDto(standUpStat);
    }

    private async Task UpdateStreak(ulong userId, Database.StandUpStat streak)
    {
        int days = (DateTime.Today.AddDays(ComparedDay - 1) - streak.LastDayOfStreak).Days;

        // Streak was already restored today.
        if (days == ComparedDay)
        {
            return;
        }

        // Streak restored using freezes.
        if (await IsValidStreak(userId, GetDto(streak)))
        {
            streak.Freezes -= days;
            streak.Streak++;
            streak.LastDayOfStreak = DateTime.Today.AddDays(ComparedDay).ToUniversalTime();

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
        // Streak was broken and there are not enough freezes available.
        else
        {
            streak.Freezes = 0;
            streak.LastDayOfStreak = DateTime.Today.AddDays(ComparedDay).ToUniversalTime();
            streak.Streak = 1;
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
        StandUpStat? stats = await GetStats(userId);
        return await IsValidStreak(userId, stats);
    }

    private async Task<bool> IsValidStreak(ulong userId, StandUpStat? streak = null)
    {
        streak ??= await GetStats(userId);

        if (streak is null)
        {
            return false;
        }

        int days = (DateTime.Today.AddDays(ComparedDay - 1) - streak.LastDayOfStreak).Days;

        return days <= streak.Freezes;
    }

    public async Task UpdateStats(ulong userId, int tasksCompleted, int tasksTotal)
    {
        bool streakMaintained = tasksCompleted >= _standUpOptions.TasksCompletedThreshold;

        Database.StandUpStat? stat = await _dbContext.StandUpStats
            .FirstOrDefaultAsync(streak => streak.UserId == userId);

        if (stat is null)
        {
            Database.StandUpStat newStat = new()
            {
                UserId = userId,
                Freezes = 0,
                LastDayOfStreak =
                    streakMaintained
                        ? DateTime.Today.AddDays(ComparedDay).ToUniversalTime()
                        : DateTime.UnixEpoch,
                Streak = streakMaintained ? 1 : 0,
                LongestStreak = streakMaintained ? 1 : 0,
                TotalCompleted = tasksCompleted,
                TotalTasks = tasksTotal
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

        if (streakMaintained)
        {
            await UpdateStreak(userId, stat);
        }

        stat.TotalCompleted += tasksCompleted;
        stat.TotalTasks += tasksTotal;

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
