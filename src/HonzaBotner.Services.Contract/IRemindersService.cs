using System;
using System.Threading.Tasks;
using HonzaBotner.Database;

namespace HonzaBotner.Services.Contract
{
    public interface IRemindersService
    {
        public Task<Reminder> CreateReminderAsync(ulong ownerId, ulong messageId, DateTime datetime, string title, string content);
    }
}
