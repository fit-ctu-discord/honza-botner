using System;

namespace HonzaBotner.Database
{
    public class Reminder
    {
        public ulong Id { get; set; }

        // Mapped Discord ID of the user that created this reminder
        public ulong OwnerId { get; set; }

        public ulong MessageId { get; set; }

        public DateTime RemindAt { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}
