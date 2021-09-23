#nullable disable
namespace HonzaBotner.Discord.Services.Options
{
    public class ReminderOptions
    {
        public static string ConfigName => "ReminderOptions";

        public string CancelEmojiName { get; set; }

        public string JoinEmojiName { get; set; }
    }
}
