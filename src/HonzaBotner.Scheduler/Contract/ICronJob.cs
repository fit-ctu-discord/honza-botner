namespace HonzaBotner.Scheduler.Contract
{
    public interface ICronJob : IJob
    {
        string CronExpression { get; }

    }
}
