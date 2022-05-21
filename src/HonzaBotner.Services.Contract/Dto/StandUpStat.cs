using System;

namespace HonzaBotner.Services.Contract.Dto;

public record StandUpStat(
    int Id, ulong UserId,
    int Streak, int LongestStreak,
    int Freezes, DateTime LastDayOfStreak,
    int TotalCompleted, int TotalTasks,
    int LastDayCompleted, int LastDayTasks
);
