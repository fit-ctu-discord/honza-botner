using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HonzaBotner.Database;
using HonzaBotner.Services.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Reminder = HonzaBotner.Services.Contract.Dto.Reminder;

namespace HonzaBotner.Services
{
    public class RemindersService : IRemindersService
    {
        private readonly HonzaBotnerDbContext _dbContext;
        private readonly ILogger<RemindersService> _logger;

        public RemindersService(HonzaBotnerDbContext dbContext, ILogger<RemindersService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Reminder> CreateReminderAsync(ulong ownerId, ulong messageId, ulong channelId,
            DateTime datetime, string content)
        {
            Database.Reminder reminder = new()
            {
                OwnerId = ownerId,
                MessageId = messageId,
                ChannelId = channelId,
                DateTime = datetime,
                Content = content
            };
            _dbContext.Reminders.Add(reminder);
            await _dbContext.SaveChangesAsync();
            return GetDto(reminder);
        }

        public async Task DeleteReminderAsync(int id)
        {
            Database.Reminder? dbReminder = await _dbContext.Reminders.FirstOrDefaultAsync(r => r.Id == id);
            if (dbReminder == null)
            {
                _logger.LogWarning("Couldn't find reminder to delete");
                return;
            }

            _dbContext.Reminders.Remove(dbReminder);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Reminder?> GetByMessageIdAsync(ulong messageId)
        {
            Database.Reminder? reminder = await _dbContext.Reminders
                .Where(reminder => reminder.MessageId == messageId)
                .FirstOrDefaultAsync();

            if (reminder == null)
                return null;

            return GetDto(reminder);
        }

        public async Task<List<Reminder>> GetRemindersToExecuteAsync(DateTime? dateTime)
        {
            var expired = await _dbContext.Reminders
                .Where(reminder => reminder.DateTime <= (dateTime ?? DateTime.Now))
                .ToListAsync();

            return expired.Select(GetDto).ToList();
        }

        public async Task DeleteExecutedRemindersAsync(DateTime? dateTime)
        {
            var expired = await _dbContext.Reminders
                .Where(reminder => reminder.DateTime <= (dateTime ?? DateTime.Now))
                .ToListAsync();

            _dbContext.Reminders.RemoveRange(expired);
            await _dbContext.SaveChangesAsync();
        }

        private static Reminder GetDto(Database.Reminder reminder) =>
            new(reminder.Id, reminder.OwnerId, reminder.MessageId, reminder.ChannelId, reminder.DateTime,
                reminder.Content);
    }
}
