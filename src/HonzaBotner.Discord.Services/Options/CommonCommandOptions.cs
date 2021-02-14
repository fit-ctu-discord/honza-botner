﻿namespace HonzaBotner.Discord.Services.Options
{
    public class CommonCommandOptions
    {
        public static string ConfigName => "CommonCommandOptions";

        public ulong MuteRoleId { get; set; }
        public string? HugEmoteName { get; set; }

        public ulong VerificationMessageId { get; set; }
        public ulong VerificationChannelId { get; set; }
        public string VerificationEmojiName { get; set; } = "✅";

        public string StaffVerificationEmojiName { get; set; } = "✅";
        public string StudentVerificationEmojiName { get; set; } = "✅";

        public ulong CustomVoiceCategory { get; set; }
        public ulong CustomVoiceClickChannel { get; set; }
        public int CustomVoiceRemoveAfterCommand { get; set; }
    }
}
