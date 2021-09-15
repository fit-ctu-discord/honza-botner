using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HonzaBotner.Services.Contract.Dto;

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
        /// <param name="content">Content of the reminder</param>
        /// <returns>The created reminder</returns>
        public Task<Reminder> CreateReminderAsync(ulong ownerId, ulong messageId, ulong channelId, DateTime datetime, string content);

        /// <summary>
        /// Cancels the specified reminder
        /// </summary>
        /// <param name="id">Reminder to be cancelled</param>
        public Task DeleteReminderAsync(int id);

        /// <summary>
        /// Find reminder bound to the specified message (embed).
        /// </summary>
        /// <param name="messageId">ID of the message</param>
        /// <returns>Reminder bound to that message (if it exists)</returns>
        public Task<Reminder?> GetByMessageIdAsync(ulong messageId);

        /// <summary>
        /// Return all reminders that should be executed.
        /// </summary>
        public Task<List<Reminder>> GetRemindersToExecuteAsync(DateTime? dateTime = null);

        /// <summary>
        /// Delete all executed reminders.
        /// </summary>
        public Task DeleteExecutedRemindersAsync(DateTime? dateTime = null);
    }
}
