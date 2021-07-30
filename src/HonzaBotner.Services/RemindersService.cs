using System;
using System.Threading.Tasks;
using HonzaBotner.Database;
using HonzaBotner.Services.Contract;

namespace HonzaBotner.Services
{
    public class RemindersService : IRemindersService
    {
        private readonly HonzaBotnerDbContext _context;

        public RemindersService(HonzaBotnerDbContext context)
        {
            _context = context;
        }

        public async Task<Reminder> CreateReminderAsync(ulong messageId)
        {
            return await Task.FromResult(new Reminder
            {
                Id = 420L,
                Title = "Test reminder",
                Content = "Content to be reminded",
                MessageId = messageId,
                DateTime = DateTime.Now.AddMinutes(30)
            });
        }
    }
}
