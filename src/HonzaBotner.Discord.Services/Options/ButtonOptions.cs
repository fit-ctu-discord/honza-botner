namespace HonzaBotner.Discord.Services.Options;

public class ButtonOptions
{
    public static string ConfigName => "ButtonOptions";

    public string? VerificationId { get; set; }
    public string? StaffVerificationId { get; set; }
    public string? StaffRemoveRoleId { get; set; }
    public ulong[]? CzechChannelsIds { get; set; }
}
