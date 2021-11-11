namespace HonzaBotner.Discord.Services.Options
{
    public class CustomVoiceOptions
    {
        public static string ConfigName => "CustomVoiceOptions";

        public ulong ClickChannelId { get; set; }
        public int RemoveAfterCommandInSeconds { get; set; }
        public ulong[] CommandChannelsIds { get; set; } = System.Array.Empty<ulong>();
    }
}
