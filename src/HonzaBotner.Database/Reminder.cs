#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace HonzaBotner.Database
{
    public class Reminder
    {
        [Key] public ulong Id { get; set; }

        // Mapped Discord ID of the user that created this reminder
        public ulong OwnerId { get; set; }

        public ulong MessageId { get; set; }

        public DateTime DateTime { get; set; }

        public string Title { get; set; }

        public string? Content { get; set; } = null;

        public Reminder(ulong ownerId, ulong messageId, DateTime dateTime, string title, string? content)
        {
            OwnerId = ownerId;
            MessageId = messageId;
            DateTime = dateTime;
            Title = title;
            Content = content;
        }

        public Reminder()
        {
            Title = "";
            Content = "";
        }
    }
}
