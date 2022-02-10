using System.Threading.Tasks;
using DSharpPlus.Entities;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Discord.Managers;

public interface IReminderManager
{
    Task<DiscordEmbed> CreateDmReminderEmbedAsync(Reminder reminder);
    Task<DiscordEmbed> CreateReminderEmbedAsync(Reminder reminder);
    Task<DiscordEmbed> CreateExpiredReminderEmbedAsync(Reminder reminder);
    Task<DiscordEmbed> CreateCanceledReminderEmbedAsync(Reminder reminder);
}
