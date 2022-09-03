using System;

namespace HonzaBotner.Services.Contract.Dto;

public record StandUpStat(
    ulong UserId, int Streak,
    int LongestStreak, int Freezes,
    DateTime LastDayOfStreak, int TotalCompleted,
    int TotalTasks, int LastDayCompleted,
    int LastDayTasks
);
