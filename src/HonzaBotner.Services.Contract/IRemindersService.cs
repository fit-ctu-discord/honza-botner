using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HonzaBotner.Database;

namespace HonzaBotner.Services.Contract
{
    public interface IRemindersService
    {
        /// <summary>
        /// Creates a new reminder with the specified parameters
        /// </summary>
        /// <param name="ownerId">ID of the owner (who called the command)</param>
        /// <param name="messageId">ID of the message that the reminder is bound to</param>
        /// <param name="channelId">ID of the channel that the message is in</param>
        /// <param name="datetime">Datetime of the reminder</param>
        /// <param name="title">Title of the reminder</param>
        /// <param name="content">Additional content of the reminder</param>
        /// <returns>The created reminder</returns>
        public Task<Reminder> CreateReminderAsync(ulong ownerId, ulong messageId, ulong channelId, DateTime datetime, string title, string? content);

        /// <summary>
        /// Cancels the specified reminder
        /// </summary>
        /// <param name="reminder">Reminder to be cancelled</param>
        public Task DeleteReminderAsync(Reminder reminder);

        /// <summary>
        /// Find reminder bound to the specified message (embed).
        /// </summary>
        /// <param name="messageId">ID of the message</param>
        /// <returns>Reminder bound to that message (if it exists)</returns>
        public Task<Reminder?> GetByMessageIdAsync(ulong messageId);

        /// <summary>
        /// Return all reminders that have should be executed (notified).
        /// </summary>
        public Task<List<Reminder>> GetRemindersThatShouldBeExecutedAsync();
    }
}
