using System;

namespace HonzaBotner.Database
{
    public class Warning
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public string Reason { get; set; }
        public DateTime IssuedAt { get; set; } = DateTime.Now;
        public ulong IssuerId { get; set; }
    }
}
