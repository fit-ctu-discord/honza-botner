using System;

namespace HonzaBotner.Database
{
    public class Verification
    {
        public Guid VerificationId { get; set; }

        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public string? CvutUsername { get; set; }
        public bool Verified { get; set; }
    }
}
