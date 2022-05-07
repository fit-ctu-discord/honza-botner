using System;
using System.Threading.Tasks;
using HonzaBotner.Database;
using HonzaBotner.Services.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StandUpStreak = HonzaBotner.Services.Contract.Dto.StandUpStreak;

namespace HonzaBotner.Services;

public class StandUpStreakService : IStandUpStreakService
{
    private readonly HonzaBotnerDbContext _dbContext;
    private readonly ILogger<StandUpStreakService> _logger;
    private const int DaysToAcquireFreeze = 6;

    public StandUpStreakService(HonzaBotnerDbContext dbContext, ILogger<StandUpStreakService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<StandUpStreak?> GetStreak(ulong userId)
    {
        Database.StandUpStreak? standUpStreak = await _dbContext.StandUpStreaks
            .FirstOrDefaultAsync(streak => streak.UserId == userId);

        return standUpStreak == null ? null : GetDto(standUpStreak);
    }

    public async Task UpdateStreak(ulong userId)
    {
        Database.StandUpStreak? streak = await _dbContext.StandUpStreaks
            .FirstOrDefaultAsync(streak => streak.UserId == userId);

        // Create new streak for a new user
        if (streak is null)
        {
            Database.StandUpStreak newStreak = new()
            {
                UserId = userId,
                Freezes = 0,
                LastDayOfStreak = DateTime.Today.AddDays(-1),
                Streak = 1,
                LongestStreak = 1
            };

            await _dbContext.StandUpStreaks.AddAsync(newStreak);

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
            _dbContext.StandUpStreaks.Update(streak);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Couldn't update streak {@Streak}", streak);
        }
    }

    public async Task<bool> IsValidStreak(ulong userId)
    {
        StandUpStreak? streak = await GetStreak(userId);

        if (streak is null)
            return false;
        int days = (DateTime.Today.AddDays(-2) - streak.LastDayOfStreak).Days;

        return days <= streak.Freezes;
    }

    private static StandUpStreak GetDto(Database.StandUpStreak standUpStreak) =>
        new StandUpStreak()
        {
            Id = standUpStreak.Id,
            UserId = standUpStreak.UserId,
            Streak = standUpStreak.Streak,
            LongestStreak = standUpStreak.LongestStreak,
            Freezes = standUpStreak.Freezes,
            LastDayOfStreak = standUpStreak.LastDayOfStreak
        };
}
