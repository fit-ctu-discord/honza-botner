using System;

namespace HonzaBotner.Database;

public class StandUpStat
{
    public int Id { get; set; }
    public ulong UserId { get; set; }
    public int Streak { get; set; }
    public int LongestStreak { get; set; }
    public int Freezes { get; set; }
    public DateTime LastDayOfStreak { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalTasks { get; set; }
}
