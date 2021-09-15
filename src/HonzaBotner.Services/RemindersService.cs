using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HonzaBotner.Database;
using HonzaBotner.Services.Contract;
using Microsoft.EntityFrameworkCore;
using Reminder = HonzaBotner.Services.Contract.Dto.Reminder;

namespace HonzaBotner.Services
{
    public class RemindersService : IRemindersService
    {
        private readonly HonzaBotnerDbContext _dbContext;

        public RemindersService(HonzaBotnerDbContext dbContext)
        {
            _dbContext = dbContext;
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
                Console.WriteLine("shit");
                return;
            }

            _dbContext.Reminders.Remove(dbReminder);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Reminder?> GetByMessageIdAsync(ulong messageId)
        {
            return GetDto(await _dbContext.Reminders
                .Where(reminder => reminder.MessageId == messageId)
                .FirstOrDefaultAsync());
        }

        public async Task<List<Reminder>> DeleteRemindersThatShouldBeExecutedAsync()
        {
            var expired = await _dbContext.Reminders
                .Where(reminder => reminder.DateTime <= DateTime.Now)
                .ToListAsync();

            _dbContext.Reminders.RemoveRange(expired);
            await _dbContext.SaveChangesAsync();

            return expired.Select(GetDto).ToList();
        }

        private static Reminder GetDto(Database.Reminder reminder) =>
            new(reminder.Id, reminder.OwnerId, reminder.MessageId, reminder.ChannelId, reminder.DateTime,
                reminder.Content);
    }
}
