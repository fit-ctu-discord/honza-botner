using System.Collections.Generic;

namespace HonzaBotner.Discord.Services.Options;

public class BadgeRoleOptions
{
    public static string ConfigName => "BadgeRoleOptions";

    public ulong[] TriggerRoles { get; set; }
    public Dictionary<ulong,ulong> PairedRoles { get; set; }
}
