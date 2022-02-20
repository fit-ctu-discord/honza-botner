using System;
using System.Collections.Generic;

namespace HonzaBotner.Discord.Services.Options;

public class BadgeRoleOptions
{
    public static string ConfigName => "BadgeRoleOptions";

    public ulong[] TriggerRoles { get; set; } = Array.Empty<ulong>();
    public Dictionary<string,ulong> PairedRoles { get; set; } = new ();
}
