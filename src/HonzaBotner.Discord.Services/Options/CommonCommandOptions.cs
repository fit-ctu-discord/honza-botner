namespace HonzaBotner.Discord.Services.Options
{
    public class CommonCommandOptions
    {
        public static string ConfigName => "CommonCommandOptions";

        public ulong MuteRoleId { get; set; }
        public string? HugEmoteName { get; set; }
    }
}
