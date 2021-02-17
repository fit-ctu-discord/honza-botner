#nullable disable
using System.Collections.Generic;

namespace HonzaBotner.Discord.Services.Options
{
    public class PinOptions
    {
        public static string ConfigName => "PinOptions";

        public int Treshold { get; set; }
        public Dictionary<string, int> RoleToWeightMapping { get; set; }
        public string EmojiName { get; set; }
    }
}
