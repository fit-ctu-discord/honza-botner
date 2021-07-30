using System.Threading.Tasks;
using HonzaBotner.Database;
using HonzaBotner.Services.Contract;

namespace HonzaBotner.Discord.Services.Jobs
{
    public class TriggerRemindersJob
    {
        private readonly IRemindersService _service;

        private readonly DiscordWrapper _guild;

        public TriggerRemindersJob(IRemindersService service, DiscordWrapper guild)
        {
            _service = service;
            _guild = guild;
        }

        public async Task Dispatch()
        {
            var reminders = await _service.GetRemindersThatShouldBeExecutedAsync();

            reminders.ForEach(SendReminderNotification);
        }

        private async void SendReminderNotification(Reminder reminder)
        {
            var channel = await _guild.Client.GetChannelAsync(0);
            var message = await channel.GetMessageAsync(reminder.MessageId);
        }
    }
}
