#nullable disable
using System.Collections.Generic;

namespace HonzaBotner.Discord.Services.Options;

public class PinOptions
{
    public static string ConfigName => "PinOptions";

    public int Threshold { get; set; }
    public Dictionary<string, int> RoleToWeightMapping { get; set; }
    public string TemporaryPinName { get; set; }
    public string PermanentPinName { get; set; }
    public string LockEmojiName { get; set; }
}
