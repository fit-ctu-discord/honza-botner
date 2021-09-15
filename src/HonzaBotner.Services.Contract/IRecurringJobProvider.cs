using System.Threading.Tasks;

namespace HonzaBotner.Services.Contract
{
    public interface IRecurringJobProvider
    {
        // Unfortunate, will be fixed with newer version of C#
        // /// <summary>
        // /// Key that will be used for registration within hangfire.
        // /// </summary>
        // /// <returns>Application-unique key</returns>
        // public string Key { get; }
        //
        // /// <summary>
        // /// Cron expression to be used within the scheduler.
        // /// </summary>
        // /// <returns>Cron expression</returns>
        // /// <seealso cref="Hangfire.Cron"/>
        // public string CronExpression { get; }

        Task Run();
    }
}
