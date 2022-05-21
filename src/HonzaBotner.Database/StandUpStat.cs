using System;
using System.ComponentModel.DataAnnotations;

namespace HonzaBotner.Database;

public class StandUpStat
{
    [Key] public ulong UserId { get; set; }
    public int Streak { get; set; }
    public int LongestStreak { get; set; }
    public int Freezes { get; set; }
    public DateTime LastDayOfStreak { get; set; }
    public int LastDayCompleted { get; set; }
    public int LastDayTasks { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalTasks { get; set; }
}
