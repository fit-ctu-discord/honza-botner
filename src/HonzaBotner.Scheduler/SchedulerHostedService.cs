using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HonzaBotner.Scheduler.Contract;

namespace HonzaBotner.Scheduler;

public class SchedulerHostedService : BackgroundService
{
    private readonly int _delay;
    private readonly ILogger<SchedulerHostedService> _logger;
    private readonly IList<CronJobWrapper> _cronJobs;

    public SchedulerHostedService(int delay, IEnumerable<ICronJob> cronJobs, ILogger<SchedulerHostedService> logger)
    {
        DateTime now = DateTime.UtcNow;

        _delay = delay;
        _logger = logger;
        _cronJobs = cronJobs
            .Select(j => new CronJobWrapper(j, j.CronExpression, now))
            .ToList();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            DateTime currentTime = DateTime.UtcNow;

            _logger.LogInformation("Scheduler running at: {Time}", currentTime.ToLocalTime());
            await RunOnceAsync(currentTime, cancellationToken);

            await Task.Delay(_delay, cancellationToken);
        }
    }

    private async Task RunOnceAsync(DateTime currentTime, CancellationToken cancellationToken)
    {
        TaskFactory taskFactory = new(TaskScheduler.Current);

        IList<CronJobWrapper> jobsToRun = _cronJobs.Where(job => job.ShouldRun(currentTime)).ToList();

        foreach (CronJobWrapper cronJob in jobsToRun)
        {
            cronJob.Next();

            await taskFactory.StartNew(async () =>
            {
                try
                {
                    _logger.LogInformation("Starting job {JobType}", cronJob.Job.Name);
                    await cronJob.Job.ExecuteAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Job {JobType} failed", cronJob.Job.Name);
                    // NOTE: Exception is not propagated, since we dont want crash scheduler.
                }
            }, cancellationToken);
        }
    }
}
