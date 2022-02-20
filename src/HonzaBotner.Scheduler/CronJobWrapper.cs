using System;
using Cronos;
using HonzaBotner.Scheduler.Contract;

namespace HonzaBotner.Scheduler;

internal class CronJobWrapper
{
    private const string CetId = "Central Europe Standard Time";

    private readonly TimeZoneInfo _centralEuropeTimezone = TimeZoneInfo.FindSystemTimeZoneById(CetId);

    public CronJobWrapper(ICronJob job, string cronExpression, DateTime nextRunTime)
    {
        Job = job;
        Expression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);
        NextRunTime = nextRunTime;
    }

    public ICronJob Job { get; }
    private CronExpression Expression { get; }

    public DateTime NextRunTime { get; private set; }
    public DateTime LastRunTime { get; private set; }

    public void Next()
    {
        LastRunTime = NextRunTime;
        NextRunTime = Expression.GetNextOccurrence(NextRunTime, _centralEuropeTimezone) ?? NextRunTime;
    }

    public bool ShouldRun(DateTime currentTime)
    {
        return NextRunTime < currentTime && LastRunTime != NextRunTime;
    }
}
