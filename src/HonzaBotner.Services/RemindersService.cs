using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HonzaBotner.Database;
using HonzaBotner.Services.Contract;
using Microsoft.EntityFrameworkCore;

namespace HonzaBotner.Services
{
    public class RemindersService : IRemindersService
    {
        private readonly HonzaBotnerDbContext _context;

        public RemindersService(HonzaBotnerDbContext context)
        {
            _context = context;
        }

        public async Task<Reminder> CreateReminderAsync(ulong ownerId, ulong messageId, ulong channelId, DateTime datetime, string content)
        {
            var reminder = new Reminder
            {
                OwnerId = ownerId,
                MessageId = messageId,
                ChannelId = channelId,
                DateTime = datetime,
                Content = content
            };

            _context.Reminders.Add(reminder);
            await _context.SaveChangesAsync();

            return reminder;
        }

        public async Task DeleteReminderAsync(Reminder reminder)
        {
            _context.Reminders.Remove(reminder);
            await _context.SaveChangesAsync();
        }

        public async Task<Reminder?> GetByMessageIdAsync(ulong messageId)
        {
            return await _context.Reminders
                .Where(reminder => reminder.MessageId == messageId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Reminder>> DeleteRemindersThatShouldBeExecutedAsync()
        {
            var expired = await _context.Reminders
                .Where(reminder => reminder.DateTime <= DateTime.Now)
                .ToListAsync();

            _context.Reminders.RemoveRange(expired);
            await _context.SaveChangesAsync();

            return expired;
        }
    }
}
