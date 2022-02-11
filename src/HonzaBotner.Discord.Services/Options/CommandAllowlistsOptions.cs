using System;

namespace HonzaBotner.Discord.Services.Options;

public enum AllowlistsTypes
{
    MemberCount
}

public class CommandAllowlistsOptions
{
    public static string ConfigName => "CommandAllowlistsOptions";

    public ulong[] MemberCount { get; set; } = Array.Empty<ulong>();

    public ulong[] GetAllowlistFromItsType(AllowlistsTypes type)
    {
        return type switch
        {
            AllowlistsTypes.MemberCount => MemberCount,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
