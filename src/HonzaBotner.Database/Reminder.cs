using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HonzaBotner.Database
{
    public class Reminder
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Mapped Discord ID of the user that created this reminder
        public ulong OwnerId { get; set; }

        public ulong MessageId { get; set; }

        public ulong ChannelId { get; set; }

        public DateTime DateTime { get; set; }

        public string Content { get; set; }
    }
}
