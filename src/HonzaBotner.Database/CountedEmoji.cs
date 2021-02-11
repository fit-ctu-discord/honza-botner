using System;

namespace HonzaBotner.Database
{
    public class CountedEmoji
    {
        public ulong Id { get; set; }
        public ulong Times { get; set; }
        public DateTime FirstUsedAt { get; set; } = DateTime.Now;
    }
}
