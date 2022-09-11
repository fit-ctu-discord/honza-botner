namespace HonzaBotner.Discord.Services.Options;

public class RolesOptions
{
    public static string ConfigName => "RolesOptions";

    public ulong Stepech { get; set; }

    public ulong Seznamka { get; set; }

    public uint RequiredVotes { get; set; }
}