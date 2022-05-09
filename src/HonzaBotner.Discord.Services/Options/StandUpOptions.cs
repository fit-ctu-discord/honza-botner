namespace HonzaBotner.Discord.Services.Options;

public class StandUpOptions
{
    public static string ConfigName => "StandUpOptions";

    public ulong StandUpRoleId { get; set; }
    public ulong StandUpChannelId { get; set; }

    public int DaysToAcquireFreeze { get; set; }
    public int TasksCompletedThreshold { get; set; }
}
