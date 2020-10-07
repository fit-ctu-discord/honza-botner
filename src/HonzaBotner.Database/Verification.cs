using System;

namespace HonzaBotner.Database
{
    public class Verification
    {
        public Guid VerificationId { get; set; }

        public ulong UserId { get; set; }
        public string? AuthId { get; set; }
        public bool Verified { get; set; }
    }
}
