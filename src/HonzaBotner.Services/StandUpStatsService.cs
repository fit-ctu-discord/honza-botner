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

    private Database.StandUpStat UpdateStreak(Database.StandUpStat streak, bool streakMaintained)
    {
        int days = (DateTime.Today.AddDays(-2) - streak.LastDayOfStreak).Days;

        // Tasks completed and on time
        if (streakMaintained && days <= streak.Freezes)
        {
            streak.Streak++;
            // streak.Streak += days + 1; // Alternative in case we want to count frozen days in streak
            streak.LastDayOfStreak = DateTime.Today.AddDays(-1).ToUniversalTime();
            streak.Freezes -= days;

            if (streak.Streak > streak.LongestStreak)
            {
                streak.LongestStreak = streak.Streak;
            }
        }
        // Tasks completed, but reset streak
        else if (streakMaintained)
        {
            streak.Streak = 1;
            streak.LastDayOfStreak = DateTime.Today.AddDays(-1).ToUniversalTime();
            streak.Freezes = 0;
        }
        // Not valid, and lateeeeeeeeeee
        else if (days >= streak.Freezes)
        {
            streak.Streak = 0;
            streak.Freezes = 0;
        }
        // Not valid && on time is ignored, in that case nothing happens

        return streak;
    }

    public async Task UpdateStats(ulong userId, int completed, int total)
    {
        bool streakMaintained = completed >= _standUpOptions.TasksCompletedThreshold;

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
                        ? DateTime.Today.AddDays(-1).ToUniversalTime()
                        : DateTime.UnixEpoch.ToUniversalTime(),
                Streak = streakMaintained ? 1 : 0,
                LongestStreak = streakMaintained ? 1 : 0,
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

        stat = UpdateStreak(stat, streakMaintained);

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
            standUpStat.UserId,
            standUpStat.Streak,
            standUpStat.LongestStreak,
            standUpStat.Freezes,
            standUpStat.LastDayOfStreak,
            standUpStat.TotalCompleted,
            standUpStat.TotalTasks,
            standUpStat.LastDayCompleted,
            standUpStat.LastDayTasks
        );
}
