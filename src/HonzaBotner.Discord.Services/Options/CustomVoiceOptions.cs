namespace HonzaBotner.Discord.Services.Options;

public class CustomVoiceOptions
{
    public static string ConfigName => "CustomVoiceOptions";

    public ulong ClickChannelId { get; set; }
    public int RemoveAfterCommandInSeconds { get; set; }
}
